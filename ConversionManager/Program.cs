using ConversionManager;
using ConversionManager.Ui;

class Program
{
    static void Main(string[] args)
    {

        IWorkerRunner runner = new ProcessRunner();
        JobManager jobManager = new JobManager(runner,3);

        ConsoleMenu menu = new ConsoleMenu(jobManager);
        menu.Run();
    }
}