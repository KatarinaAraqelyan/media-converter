namespace ConversionManager;

public interface IWorkerRunner
{
    void Run(Job job, CancellationToken token);
}