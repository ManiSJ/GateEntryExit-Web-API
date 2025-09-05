using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Newtonsoft.Json;

namespace GateEntryExit.BackgroundJobs
{
    public class PreventDuplicateEnqueueFilter : JobFilterAttribute, IClientFilter
    {
        public void OnCreating(CreatingContext filterContext)
        {
            var job = filterContext.Job;
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            // Serialize job arguments to a string (customize as needed)
            string jobArgsFingerprint = GenerateFingerprint(job);

            // Check Enqueued Jobs
            var queues = monitoringApi.Queues();
            foreach (var queue in queues)
            {
                var enqueuedJobs = monitoringApi.EnqueuedJobs(queue.Name, 0, int.MaxValue);
                foreach (var enqueuedJob in enqueuedJobs)
                {
                    if (IsDuplicate(job, enqueuedJob.Value.Job, jobArgsFingerprint))
                    {
                        filterContext.Canceled = true;
                        return;
                    }
                }
            }

            // Check Processing Jobs
            var processingJobs = monitoringApi.ProcessingJobs(0, int.MaxValue);
            foreach (var processingJob in processingJobs)
            {
                if (IsDuplicate(job, processingJob.Value.Job, jobArgsFingerprint))
                {
                    filterContext.Canceled = true;
                    return;
                }
            }
        }

        public void OnCreated(CreatedContext filterContext)
        {
            // No action needed after creation
        }

        private bool IsDuplicate(Job newJob, Job existingJob, string newArgsFingerprint)
        {
            if (existingJob == null || newJob == null)
                return false;

            if (existingJob.Method.Name != newJob.Method.Name ||
                existingJob.Type != newJob.Type)
                return false;

            string existingArgsFingerprint = string.Join(",", existingJob.Args.Select(a => a?.ToString()));
            return existingArgsFingerprint == newArgsFingerprint;
        }

        private string GenerateFingerprint(Job job)
        {
            var methodName = $"{job.Type.FullName}.{job.Method.Name}";
            var serializedArgs = JsonConvert.SerializeObject(job.Args);
            return $"{methodName}:{serializedArgs}";
        }
    }
}
