using GateEntryExit.DatabaseContext;
using GateEntryExit.Domain;
using GateEntryExit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GateEntryExit.Repositories
{
    public class GateEntryRepository : BaseRepository, IGateEntryRepository
    {
        public GateEntryRepository(GateEntryExitDbContext dbContext) : base(dbContext)
        {
            
        }

        public IQueryable<GateEntry> GetAll()
        {
            return _dbContext.GateEntries.Include(p => p.Gate).Select(p => p);
        }

        public async Task<GateEntry> GetAsync(Guid id)
        {
            return await _dbContext.GateEntries.Include(p => p.Gate).Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task InsertAsync(GateEntry gateEntry)
        {
            await _dbContext.GateEntries.AddAsync(gateEntry);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, DateTime timeStamp, int numberOfPeople)
        {
            var gateEntry = await _dbContext.GateEntries.FindAsync(id);
            if (gateEntry != null)
            {
                gateEntry.TimeStamp = timeStamp;
                gateEntry.NumberOfPeople = numberOfPeople;
                _dbContext.GateEntries.Update(gateEntry);
                await SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var gateEntry = await _dbContext.GateEntries.FindAsync(id);
            if (gateEntry != null)
            {
                _dbContext.GateEntries.Remove(gateEntry);
                await SaveChangesAsync();
            }
        }
    }
}
