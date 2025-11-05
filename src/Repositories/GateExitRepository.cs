using GateEntryExit.DatabaseContext;
using GateEntryExit.Domain;
using GateEntryExit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GateEntryExit.Repositories
{
    public class GateExitRepository : BaseRepository, IGateExitRepository
    {
        public GateExitRepository(GateEntryExitDbContext dbContext) : base(dbContext)
        {
            
        }

        public IQueryable<GateExit> GetAll()
        {
            return  _dbContext.GateExits.Include(p => p.Gate).Select(p => p);
        }

        public async Task<GateExit> GetAsync(Guid id)
        {
            return await _dbContext.GateExits.Include(p => p.Gate).Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task InsertAsync(GateExit gateExit)
        {
            await _dbContext.GateExits.AddAsync(gateExit);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, DateTime timeStamp, int numberOfPeople)
        {
            var gateExit = await _dbContext.GateExits.FindAsync(id);
            if (gateExit != null)
            {
                gateExit.TimeStamp = timeStamp;
                gateExit.NumberOfPeople = numberOfPeople;
                _dbContext.GateExits.Update(gateExit);
                await SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var gateExit = await _dbContext.GateExits.FindAsync(id);
            if (gateExit != null)
            {
                _dbContext.GateExits.Remove(gateExit);
                await SaveChangesAsync();
            }
        }
    }
}
