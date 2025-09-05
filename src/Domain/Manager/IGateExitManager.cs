namespace GateEntryExit.Domain.Manager
{
    public interface IGateExitManager
    {
        GateExit Create(Guid id, Guid gateId, int numberOfPeople, DateTime timeStamp);
    }
}
