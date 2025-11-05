namespace GateEntryExit.Domain.Manager
{
    public interface ISensorManager
    {
        Sensor Create(Guid id, Guid gateId, string name);
    }
}
