using GateEntryExit.Domain;

namespace GateEntryExit.Repositories.Interfaces
{
    public interface IGateExitRepository
    {
        IQueryable<GateExit> GetAll();

        Task<GateExit> GetAsync(Guid id);

        Task InsertAsync(GateExit input);

        Task UpdateAsync(Guid id, DateTime timeStamp, int numberOfPeople);

        Task DeleteAsync(Guid id);
    }
}
