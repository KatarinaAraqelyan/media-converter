namespace ConversionManager;

public class Job
{
    public Guid Id { get; }
    public int ShortId { get; }

    public string Input { get; }
    public string Output { get; }
    public string Options { get; }

    public JobStatus Status { get; set; }
    public int Progress { get; set; }

    public Job(int shortId, string input, string output, string options)
    {
        Id = Guid.NewGuid();
        ShortId = shortId;
        Input = input;
        Output = output;
        Options = options;
        Status = JobStatus.Queued;
        Progress = 0;
    }
}