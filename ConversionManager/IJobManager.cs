namespace ConversionManager;
public interface IJobManager
{
    Job AddJob(string input, string output, string options);
    bool CancelJob(int shortId);
    int CancelAll();
    List<Job> GetAllJobs();
    Job? FindByShortId(int shortId);
    void WaitForAllJobsToFinish();
}