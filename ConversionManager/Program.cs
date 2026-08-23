namespace ConversionManager
{
    internal class Program
    {
        
        static void Main(string[] args)
        {

            var runner = new SimulateWorkerRunner();
            var jobManager = new JobManager(runner, workerCount: 2);

            jobManager.JobChanged += job =>
            {
                Console.WriteLine($"[EVENT] Job {job.ShortId} -> {job.Status} ({job.Progress}%)");
            };

            var job1 = jobManager.AddJob("input1.mp4", "output1.mp4", "opt");
            var job2 = jobManager.AddJob("input2.mp4", "output2.mp4", "opt");
            var job3 = jobManager.AddJob("input3.mp4", "output3.mp4", "opt");

            Console.WriteLine("All jobs added, waiting a bit then canceling job 3...");
            Thread.Sleep(200);

            bool canceled = jobManager.CancelJob(job3.ShortId);
            Console.WriteLine($"Cancel job {job3.ShortId} result: {canceled}");

            Console.WriteLine("Waiting for all jobs to finish...");
            jobManager.WaitForAllJobsToFinish();

            Console.WriteLine("\nFinal states:");
            foreach (var job in jobManager.GetAllJobs())
            {
                Console.WriteLine($"Job {job.ShortId}: {job.Status}, {job.Progress}%");
            }
        }
    }
}
