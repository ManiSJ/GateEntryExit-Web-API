using GateEntryExit.BackgroundJobServices.Interfaces;
using Hangfire;

namespace GateEntryExit.BackgroundJobs
{
    public class GateEntryExitBackgroundJob
    {
        private readonly IGateEntryExitBackgroundJobService _gateEntryExitBackgroundJobService;

        public GateEntryExitBackgroundJob(IGateEntryExitBackgroundJobService gateEntryExitBackgroundJobService)
        {
            _gateEntryExitBackgroundJobService = gateEntryExitBackgroundJobService;
        }

        [AutomaticRetry(Attempts =0)]
        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public void Execute()
        {
            try
            {
                _gateEntryExitBackgroundJobService.Execute();
            }
            catch
            {
                throw;
            }
        }
    }
}
