using GateEntryExit.Domain;

namespace GateEntryExit.Repositories.Interfaces
{
    public interface ISensorRepository
    {
        IQueryable<Sensor> GetAll();

        Task<Sensor> GetAsync(Guid id);

        Task<bool> IsGateAlreadyHasSensorAsync(Guid gateId);

        IQueryable<Sensor> GetAllWithDetails();

        Task InsertAsync(Sensor sensor);

        Task UpdateAsync(Guid id, string name);

        Task DeleteAsync(Guid id);
    }
}
