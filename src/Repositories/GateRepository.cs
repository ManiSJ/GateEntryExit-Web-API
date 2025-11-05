using GateEntryExit.DatabaseContext;
using GateEntryExit.Domain;
using GateEntryExit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GateEntryExit.Repositories
{
    public class GateRepository : BaseRepository, IGateRepository
    {
        public GateRepository(GateEntryExitDbContext dbContext) : base(dbContext)
        {
            
        }

        public IQueryable<Gate> GetAll()
        {
             return _dbContext.Gates.Select(p => p);
        }

        public IQueryable<Gate> GetAll(Guid[] gateIds)
        {
            return _dbContext.Gates.Where(p => gateIds.Contains(p.Id));
        }

        public async Task<Gate> GetAsync(string name)
        {
            return await _dbContext.Gates.Where(p => p.Name == name).FirstOrDefaultAsync();
        }

        public async Task<Gate> GetAsync(Guid id)
        {
            return await _dbContext.Gates.Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var isNameUnique = true;

            var recordCount = await _dbContext.Gates.CountAsync();

            if (recordCount > 1)
            {
                var gateNameCount = await _dbContext.Gates.Where(p => p.Name.Trim().ToLower() == name.Trim().ToLower()).CountAsync();

                if(gateNameCount >= 1)
                {
                    isNameUnique = false;
                }
            }

            return isNameUnique;
        }

        public async Task InsertAsync(Gate gate)
        {
            await _dbContext.Gates.AddAsync(gate);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, string name)
        {
            var gate = await _dbContext.Gates.FindAsync(id);
            if(gate != null)
            {
                gate.Name = name;
                _dbContext.Gates.Update(gate);
                await SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var gate = await _dbContext.Gates.FindAsync(id);
            if (gate != null)
            {
                _dbContext.Gates.Remove(gate);
                await SaveChangesAsync();
            }
        }
    }
}
