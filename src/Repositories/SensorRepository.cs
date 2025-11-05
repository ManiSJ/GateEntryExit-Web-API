using GateEntryExit.DatabaseContext;
using GateEntryExit.Domain;
using GateEntryExit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace GateEntryExit.Repositories
{
    public class SensorRepository : BaseRepository, ISensorRepository
    {
        public SensorRepository(GateEntryExitDbContext dbContext) : base(dbContext)
        {
             
        }

        public IQueryable<Sensor> GetAll()
        {
            return _dbContext.Sensors.Include(p => p.Gate).Select(p => p);

            //return await _dbContext.Sensors
            //    .Include(p => p.Gate)
            //    .Select(s => new Sensor() { Id = s.Id, GateId = s.GateId, Gate = s.Gate })
            //    .ToListAsync(); 
        }

        public async Task<Sensor> GetAsync(Guid id)
        {
            return await _dbContext.Sensors.Include(p => p.Gate).Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> IsGateAlreadyHasSensorAsync(Guid gateId)
        {
            return await _dbContext.Sensors.Where(p => p.GateId == gateId).CountAsync() >= 1;
        }

        public IQueryable<Sensor> GetAllWithDetails() 
        {
            return _dbContext
                   .Sensors
                   .Include(p => p.Gate)
                       .ThenInclude(p => p.GateEntries)
                   .Include(p => p.Gate)
                       .ThenInclude(p => p.GateExits)
                   .Select(p => p);
        }

        public async Task InsertAsync(Sensor sensor)
        {
            await _dbContext.Sensors.AddAsync(sensor);
            //await _dbContext.Database.ExecuteSqlRawAsync("INSERT INTO Sensors (Id, GateId) VALUES (@p0, @p1)", sensor.Id, sensor.GateId);

            await SaveChangesAsync();
        }
        public async Task UpdateAsync(Guid id, string name)
        {
            var sensor = await _dbContext.Sensors.FindAsync(id);
            if (sensor != null)
            {
                sensor.Name = name;
                _dbContext.Sensors.Update(sensor);
                await SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var sensor = await _dbContext.Sensors.FindAsync(id);
            if (sensor != null)
            {
                _dbContext.Sensors.Remove(sensor);
                await SaveChangesAsync();
            }
        }
    }
}
