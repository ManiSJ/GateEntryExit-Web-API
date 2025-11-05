using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Newtonsoft.Json;

namespace GateEntryExit.BackgroundJobs
{
    public class PreventDuplicateProcessingFilter : JobFilterAttribute, IServerFilter
    {
        public void OnPerforming(PerformingContext context)
        {
            var currentJob = context.BackgroundJob?.Job;
            var currentJobId = context.BackgroundJob?.Id;

            if (currentJob == null || string.IsNullOrWhiteSpace(currentJobId))
                return;

            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var fingerprint = GenerateFingerprint(currentJob);

            // Get all processing jobs
            var processingJobs = monitoringApi.ProcessingJobs(0, 100);
            foreach (var job in processingJobs)
            {
                if (job.Key == currentJobId)
                    continue; // Skip current job

                var existingJob = job.Value.Job;
                if (existingJob != null && GenerateFingerprint(existingJob) == fingerprint)
                {
                    // Cancel this job because a duplicate is already processing
                    context.Canceled = true;
                    return;
                }
            }
        }

        public void OnPerformed(PerformedContext context)
        {
            // No-op
        }

        private string GenerateFingerprint(Job job)
        {
            var methodName = $"{job.Type.FullName}.{job.Method.Name}";
            var serializedArgs = JsonConvert.SerializeObject(job.Args);
            return $"{methodName}:{serializedArgs}";
        }
    }
}
