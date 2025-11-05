using GateEntryExit.Domain;

namespace GateEntryExit.Repositories.Interfaces
{
    public interface IGateRepository
    {
        IQueryable<Gate> GetAll();

        IQueryable<Gate> GetAll(Guid[] gateIds);

        Task<Gate> GetAsync(string name);

        Task<Gate> GetAsync(Guid id);

        Task<bool> IsNameUniqueAsync(string name);

        Task InsertAsync(Gate gate);

        Task UpdateAsync(Guid id, string name);

        Task DeleteAsync(Guid id);
    }
}
