namespace GateEntryExit.Domain.Manager
{
    public class GateExitManager : IGateExitManager
    {
        public GateExitManager()
        {
            
        }

        public GateExit Create(Guid id, Guid gateId, int numberOfPeople, DateTime timeStamp)
        {
            return new GateExit(id, gateId, numberOfPeople, timeStamp);
        }
    }
}
