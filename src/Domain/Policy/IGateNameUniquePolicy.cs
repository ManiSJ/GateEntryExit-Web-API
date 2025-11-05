namespace GateEntryExit.Domain.Policy
{
    public interface IGateNameUniquePolicy
    {
        Task<bool> IsNameUniqueAsync(string name);
    }
}
