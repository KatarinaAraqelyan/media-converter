namespace ConversionManager.Ui;

public sealed class ConsoleMenu
{
    private readonly JobManager _jobManager;
    private bool _exitRequested;

    public ConsoleMenu(JobManager jobManager)
    {
        _jobManager = jobManager;
    }

    public void Run()
    {
        Console.WriteLine("Conversion Manager ready. Use arrow keys to navigate, Enter to select, or press a digit.");

        while (!_exitRequested)
        {
            string? choice = PromptForChoiceInteractive();
            HandleChoice(choice?.Trim());
        }
    }
    private string? PromptForChoiceInteractive()
    {
        string[] labels = {
            "1) Add job",
            "2) Monitor progress (live)",
            "3) Cancel one job",
            "4) Cancel all jobs",
            "5) List jobs",
            "6) Wait until all jobs finish",
            "7) Help",
            "0) Exit"
        };

        int selected = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("==== Conversion Manager ====");
            Console.WriteLine("Use Up/Down arrows to navigate and Enter to select.");
            Console.WriteLine();

            for (int i = 0; i < labels.Length; i++)
            {
                if (i == selected)
                {
                    Console.Write("> ");
                    Console.WriteLine(labels[i]);
                }
                else
                {
                    Console.Write("  ");
                    Console.WriteLine(labels[i]);
                }
            }

            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.UpArrow)
            {
                selected = (selected - 1 + labels.Length) % labels.Length;
                continue;
            }

            if (key.Key == ConsoleKey.DownArrow)
            {
                selected = (selected + 1) % labels.Length;
                continue;
            }

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                Console.WriteLine(labels[selected]);
                char c = labels[selected][0];
                return c.ToString();
            }
        }
    }

    private void HandleChoice(string? choice)
    {
        switch (choice)
        {
            case "1": AddJob(); break;
            case "2": new LiveMonitor(_jobManager).Run(); break;
            case "3": CancelOne(); break;
            case "4": CancelAll(); break;
            case "5": ListJobs(); break;
            case "6": WaitForAll(); break;
            case "7": PrintHelp(); break;
            case "0": Exit(); break;
            default:
                Console.WriteLine("Unknown option. Type 7 for help.");
                break;
        }
    }

    private void AddJob()
    {
        Console.Write("Input path/name: ");
        string input = (Console.ReadLine() ?? "").Trim();

        Console.Write("Output path/name: ");
        string output = (Console.ReadLine() ?? "").Trim();

        Console.Write("Options (free text, can be empty): ");
        string options = (Console.ReadLine() ?? "").Trim();

        if (input.Length == 0 || output.Length == 0)
        {
            Console.WriteLine("Input and output cannot be empty - job not added.");
            return;
        }

        Job job = _jobManager.AddJob(input, output, options);
        Console.WriteLine($"Added job {job.ShortId} (Queued).");
        WaitForUser();
    }

    private void CancelOne()
    {
        List<Job> jobs = _jobManager.GetAllJobs();
        var cancellable = jobs.FindAll(j => j.Status == JobStatus.Queued || j.Status == JobStatus.Running);

        if (cancellable.Count == 0)
        {
            Console.WriteLine("There are no queued or running jobs to cancel.");
            WaitForUser();
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Jobs that can be canceled:");
        foreach (Job job in cancellable)
        {
            Console.WriteLine(job);
        }

        Console.Write("Short id of the job to cancel: ");
        string idText = (Console.ReadLine() ?? "").Trim();

        Job? jobToCancel = cancellable.Find(j => string.Equals(j.ShortId.ToString(), idText));
        if (jobToCancel is null)
        {
            Console.WriteLine("No cancellable job found with that id.");
            WaitForUser();
            return;
        }

        bool wasCanceled = _jobManager.CancelJob(jobToCancel.ShortId);
        Console.WriteLine(wasCanceled
            ? $"Cancellation requested for {jobToCancel.ShortId}."
            : $"Unable to cancel {jobToCancel.ShortId} (it may have finished just now).");
        WaitForUser();
        return;
    }

    private void CancelAll()
    {
        int count = _jobManager.CancelAll();
        if (count == 0)
        {
            Console.WriteLine("There are no queued or running jobs to cancel.");
            WaitForUser();
            return;
        }
        else
        {
            Console.WriteLine($"Cancellation requested for {count} job(s).");
            WaitForUser();
            return;
        }
    }

    private void ListJobs()
    {
        List<Job> jobs = _jobManager.GetAllJobs();
        if (jobs.Count == 0)
        {
            Console.WriteLine("No jobs yet.");
            WaitForUser();
            return;
        }

        Console.WriteLine();
        foreach (Job job in jobs)
        {
            string extra = (job.Status == JobStatus.Queued || job.Status == JobStatus.Running) ? " [cancellable]" : string.Empty;
            Console.WriteLine(job + extra);
        }
        WaitForUser();
    }

    private void WaitForAll()
    {
        List<Job> jobs = _jobManager.GetAllJobs();
        if (jobs.Count == 0)
        {
            Console.WriteLine("No jobs yet - nothing to wait for.");
            WaitForUser();
            return;
        }

        Console.WriteLine("Waiting for every job to reach a final state (this blocks the menu)...");
        _jobManager.WaitForAllJobsToFinish();
        Console.WriteLine("All jobs finished:");
        ListJobs();
    }

    private static void PrintHelp()
    {
        Console.WriteLine();
        Console.WriteLine("==== Help ====");
        Console.WriteLine("1) Add job          Queue a new mock conversion job (input/output/options are");
        Console.WriteLine("                     free text - no real files are touched).");
        Console.WriteLine("2) Monitor progress  Live-updating table of every job's status and percent.");
        Console.WriteLine("                     Press any key to return here.");
        Console.WriteLine("3) Cancel one job    Cancel a specific job by its short id.");
        Console.WriteLine("                       - Queued jobs are removed before they ever start.");
        Console.WriteLine("                       - Running jobs have their worker process killed.");
        Console.WriteLine("4) Cancel all jobs   Cancels every job that is still Queued or Running.");
        Console.WriteLine("5) List jobs         One-time snapshot of every job and its status.");
        Console.WriteLine("6) Wait until finish Blocks until every job is Completed/Failed/Canceled.");
        Console.WriteLine("0) Exit              Stops worker threads and quits. If jobs are still");
        Console.WriteLine("                     Running, exit waits for them to finish naturally -");
        Console.WriteLine("                     cancel them first (option 4) for a fast exit.");
        Console.WriteLine();
        Console.WriteLine("Press any key to return to menu...");
        Console.ReadKey(intercept: true);
    }

    private void WaitForUser()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to return to menu...");
        Console.ReadKey(intercept: true);
    }

    private void Exit()
    {
        Console.WriteLine("Shutting down worker threads ...");
        _jobManager.CancelAll();
        _exitRequested = true;
    }
}
