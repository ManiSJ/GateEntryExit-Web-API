using Hangfire.Common;
using Hangfire.Server;

namespace GateEntryExit.BackgroundJobs
{
    public class BackgroundJobPauseFilter : JobFilterAttribute, IServerFilter
    {
        public static bool IsPaused { get; set; } = false;

        public void OnPerforming(PerformingContext performingContext)
        {
            if (IsPaused)
            {
                performingContext.Canceled = true;
            }
        }

        public void OnPerformed(PerformedContext performedContext)
        {

        }
    }
}
