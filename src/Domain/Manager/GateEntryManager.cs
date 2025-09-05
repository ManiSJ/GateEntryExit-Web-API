using GateEntryExit.Domain;

namespace GateEntryExit.Domain.Manager
{
    public class GateEntryManager : IGateEntryManager
    {
        public GateEntryManager()
        {
            
        }

        public GateEntry Create(Guid id, Guid gateId, int numberOfPeople, DateTime timeStamp)
        {
            return new GateEntry(id, gateId, numberOfPeople, timeStamp);
        }
    }
}
