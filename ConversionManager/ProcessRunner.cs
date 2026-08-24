using System.Diagnostics;

namespace ConversionManager;

public class ProcessRunner : IWorkerRunner
{
    public void Run(Job job, CancellationToken token)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "/Users/katharinaaraqelyan/Desktop/media-converter/MockConverter/bin/Debug/net10.0/MockConverter",
            Arguments = $"--input \"{job.Input}\" --output \"{job.Output}\" --options \"{job.Options}\"",
            UseShellExecute = false,       
            RedirectStandardOutput = true,  
            CreateNoWindow = true,
        };

        
        Process? process = null;

        try
        {
            process = Process.Start(startInfo);

            if (process == null)
            {
                job.Status = JobStatus.Failed;
                return;
            }

            job.Status = JobStatus.Running;

            using var registration = token.Register(() =>
            {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
            });

            bool isOk = false;
            string? line;

            while ((line = process.StandardOutput.ReadLine()) != null)
            {
                if (line.StartsWith("PROGRESS "))
                {
                    string[] parts = line.Split(' ');

                    if (parts.Length > 1 && int.TryParse(parts[1], out int value))
                    {
                        job.Progress = value;
                        Console.WriteLine(job);
                    }
                }
                if (line.StartsWith("DONE OK"))
                {
                    isOk = true;
                }
            }

            process.WaitForExit();

            if (token.IsCancellationRequested)
            {
                job.Status = JobStatus.Canceled;
            }
            else if (process.ExitCode == 0 && isOk)
            {
                job.Status = JobStatus.Completed;
            }
            else
            {
                job.Status = JobStatus.Failed;
            }
        }
        catch (Exception)
        {
            job.Status = JobStatus.Failed;
        }
    }
}