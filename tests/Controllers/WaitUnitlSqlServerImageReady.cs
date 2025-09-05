using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;

namespace GateEntryExit.Test.Controllers
{
    public class WaitUnitlSqlServerImageReady : IWaitUntil
    {
        public async Task<bool> UntilAsync(IContainer container)
        {
            try
            {
                var connString = $"Data Source=localhost, 65473;Initial Catalog=GateEntryExit;User ID=sa;Password=OhLord;Trusted Server Certificate=True;";
                using var connection = new SqlConnection(connString);
                await connection.OpenAsync();

                using var cmd = new SqlCommand("SELECT 1", connection);
                await cmd.ExecuteScalarAsync();

                return true;
            }
            catch(Exception ex)
            {
                await Task.Delay(2000);
                return false;
            }
        }
    }
}
