namespace ConversionManager;


public class JobManager : IJobManager
{
    public event Action<Job>? JobChanged;

    private readonly Queue<Job> _queue = new();

    private readonly List<Job> _allJobs = new();

    private readonly object _lock = new();

    private readonly Thread[] _workers;

    private readonly IWorkerRunner _runner;

    private readonly Dictionary<int, CancellationTokenSource> _tokens = new();

    private int _nextShortId = 1;
    private bool _running = true;

    public JobManager(IWorkerRunner runner, int workerCount)
    {
        _runner = runner;
        _workers = new Thread[workerCount];

        for (int i = 0; i < workerCount; i++)
        {
            _workers[i] = new Thread(WorkerLoop);
            _workers[i].IsBackground = true;
            _workers[i].Start();
        }
    }


    private void WorkerLoop()
    {
        while (_running)
        {
            Job job;
            CancellationToken token;


            lock (_lock)
            {
                while (_queue.Count == 0)
                {
                    // this is working with Monitor.Pulse
                    Monitor.Wait(_lock);
                }

                job = _queue.Dequeue();

                if (job.Status == JobStatus.Canceled)
                {
                    continue;
                }

                token = _tokens[job.ShortId].Token;

                job.Status = JobStatus.Running;
                
            }

            JobChanged?.Invoke(job);

            try
            {
                _runner.Run(job, token);

                lock (_lock)
                {
                    Monitor.PulseAll(_lock);

                }
            }
            catch (OperationCanceledException)
            {
                lock (_lock)
                {
                    job.Status = JobStatus.Canceled;
                    Monitor.PulseAll(_lock);

                }
            }
            catch (Exception)
            {
                lock (_lock)
                {
                    job.Status = JobStatus.Failed;
                    Monitor.PulseAll(_lock);

                }
            }

            JobChanged?.Invoke(job);
        }
    }

    
    
    
    
    // helper methods
    public Job AddJob(string input, string output, string options)
    {
        Job job;

        lock (_lock)
        {
            job = new Job(_nextShortId, input, output, options);
            _nextShortId++;

            _tokens[job.ShortId] = new CancellationTokenSource();

            _allJobs.Add(job);

            _queue.Enqueue(job);

            Monitor.Pulse(_lock); // wake one sleeping worker
        }

        return job;
    }


    public bool CancelJob(int shortId)
    {
        lock (_lock)
        {
            //FirstOrDefault is LINQ |=> give me back the first item that matches some condition.
            var job = _allJobs.FirstOrDefault(j => j.ShortId == shortId);
            if (job == null)
                return false;

            if (job.Status == JobStatus.Queued)
            {
                job.Status = JobStatus.Canceled;
                return true;
            }

            if (job.Status == JobStatus.Running)
            {
                _tokens[job.ShortId].Cancel();
                return true;
            }

            return false;
        }
    }


    public int CancelAll()
    {
        int count = 0;

        lock (_lock)
        {
            foreach (var job in _allJobs)
            {
                if (job.Status == JobStatus.Queued)
                {
                    job.Status = JobStatus.Canceled;
                    count++;
                }
                else if (job.Status == JobStatus.Running)
                {
                    _tokens[job.ShortId].Cancel();
                    count++;
                }
            }
        }

        return count;
    }

    public List<Job> GetAllJobs()
    {
        lock (_lock)
        {
            return new List<Job>(_allJobs);
        }
    }

    public Job? FindByShortId(int shortId)
    {
        lock (_lock)
        {
            return _allJobs.FirstOrDefault(j => j.ShortId == shortId);
        }
    }

    public void WaitForAllJobsToFinish()
    {
        lock (_lock)
        {
            while (_queue.Count > 0 || _allJobs.Any(j => j.Status == JobStatus.Running))
            {
                Monitor.Wait(_lock);
            }
        }
    }
}