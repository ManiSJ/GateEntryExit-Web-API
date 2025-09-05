namespace GateEntryExit.Domain.Manager
{
    public interface IGateEntryManager
    {
        GateEntry Create(Guid id, Guid gateId, int numberOfPeople, DateTime timeStamp);
    }
}
