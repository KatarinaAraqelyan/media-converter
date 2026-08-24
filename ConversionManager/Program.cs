using ConversionManager;
class Program
{
    static void Main(string[] args)
    {
        var testJob = new Job(1, "test_video.mp4", "test_video.mp3", "fast");
        var runner = new ProcessRunner();
        using var cancellation = new CancellationTokenSource();
        runner.Run(testJob, cancellation.Token);
    }
}