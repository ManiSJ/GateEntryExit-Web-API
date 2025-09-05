using GateEntryExit.DatabaseContext;

namespace GateEntryExit.Repositories
{
    public class BaseRepository
    {
        protected readonly GateEntryExitDbContext _dbContext;

        public BaseRepository(GateEntryExitDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChangesAsync()
        {
           await _dbContext.SaveChangesAsync();
        }
    }
}
