using GateEntryExit.Domain;

namespace GateEntryExit.Repositories.Interfaces
{
    public interface IGateEntryRepository
    {
        IQueryable<GateEntry> GetAll();

        Task<GateEntry> GetAsync(Guid id);

        Task InsertAsync(GateEntry input);

        Task UpdateAsync(Guid id, DateTime timeStamp, int numberOfPeople);

        Task DeleteAsync(Guid id);
    }
}
