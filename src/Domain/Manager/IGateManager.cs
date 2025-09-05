namespace GateEntryExit.Domain.Manager
{
    public interface IGateManager
    {
        Task<Gate> CreateAsync(Guid id, string name);
    }
}
