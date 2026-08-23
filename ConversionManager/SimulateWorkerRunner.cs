namespace ConversionManager;

public class SimulateWorkerRunner : IWorkerRunner
{
    public void Run(Job job, CancellationToken token)
    {
        for (int i = 1; i <= 10; i++)
        {
            token.ThrowIfCancellationRequested();

            Thread.Sleep(300);

            job.Progress = i * 10;
        }
    }
}