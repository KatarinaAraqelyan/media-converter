namespace ConversionManager.Ui;
public sealed class LiveMonitor
{
    private readonly JobManager _jobManager;

    public LiveMonitor(JobManager jobManager)
    {
        _jobManager = jobManager;
    }

    public void Run()
    {
        Console.WriteLine();
        Console.WriteLine("Entering live monitor - press any key to return to the menu.");
        Thread.Sleep(1500);

        while (!Console.KeyAvailable)
        {
            Render();
            Thread.Sleep(300);
        }

        Console.ReadKey(intercept: true);
    }

    private void Render()
    {
        Console.Clear();
        Console.WriteLine("==== Live Monitor - press any key to exit ====");
        Console.WriteLine($"{"Id",-10} {"Status",-10} {"Progress",-16} {"Input",-18} {"Output",-18}");
        Console.WriteLine(new string('-', 76));

        var jobs = _jobManager.GetAllJobs();
        if (jobs.Count == 0)
        {
            Console.WriteLine("(no jobs yet - add one from the main menu)");
            return;
        }

        foreach (Job job in jobs)
        {
            string bar = RenderBar(job.Progress);
            Console.WriteLine(
                $"{job.ShortId,-10} {job.Status,-10} {bar,-16} {Truncate(job.Input, 18),-18} {Truncate(job.Output, 18),-18}");
        }
    }

    private static string RenderBar(int percent)
    {
        const int width = 10;
        int filled = Math.Clamp(percent * width / 100, 0, width);
        return $"[{new string('#', filled)}{new string('.', width - filled)}]{percent,4}%";
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Length <= maxLength ? text : text[..(maxLength - 1)] + "…";
    }
}
