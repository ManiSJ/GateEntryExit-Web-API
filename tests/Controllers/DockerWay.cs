using Docker.DotNet;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using GateEntryExit.DatabaseContext;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Testcontainers.MsSql;

namespace GateEntryExit.Test.Controllers
{
    public class DockerWay : TestWay, ITestWay, IAsyncLifetime
    {
        private MsSqlContainer _msSqlContainer;
        private IContainer _webApiContainer;
        private IContainer _webUiContainer;
        private IContainer _playwrightContainer;

        public DockerWay()
        {
            
        }

        public HttpClient CreateClient() => new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

        public async Task InitializeAsync()
        {
            await CreateNetworkAsync();
            await CreateSqlContainerAsync();
            await ApplyPendingMigrationsAsync();
            //await RestoreDatabaseAsync();
            await CreateWebApiContainerAsync();
            await WaitUntilApiReadyAsync();
            await CreateWebUiContainerAsync();
            await CreatePlaywrightContainerAsync();
        }

        private async Task CreatePlaywrightContainerAsync()
        {
            try
            {
                string hostPath = Path.GetFullPath("../../src/ui/test-results");
                _playwrightContainer = new ContainerBuilder()
                                    .WithImage("gate-entry-exit-playwright-image")
                                    .WithName("gate-entry-exit-playwright-container")
                                    .WithNetwork("dockerNetwork")
                                    .WithBindMount(hostPath, "/app/test-results")
                                    .WithEnvironment("BASE_URL", "http://gate-entry-exit-ui-container/")
                                    .WithEnvironment("IsInDocker", "true")
                                    .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Tests running finished"))
                                    .DependsOn(_webUiContainer)
                                    .Build();
                await _playwrightContainer.StartAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task CreateWebUiContainerAsync()
        {
            try
            {
                _webUiContainer = new ContainerBuilder()
                                    .WithImage("gate-entry-exit-ui-image")
                                    .WithName("gate-entry-exit-ui-container")
                                    .WithPortBinding(4201, 4200)
                                    .WithWaitStrategy(Wait.ForUnixContainer()
                                        .UntilHttpRequestIsSucceeded(request => request.ForPort(4200).ForPath("/")))
                                    .WithNetwork("dockerNetwork")
                                    .DependsOn(_webApiContainer)
                                    .Build();
                await _webUiContainer.StartAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task WaitUntilApiReadyAsync(int retries = 10)
        {
            try
            {
                using var httpClient = new HttpClient();
                for(int i = 0; i < retries; i++)
                {
                    try
                    {
                        var response = await httpClient.GetAsync("");
                        if (response.IsSuccessStatusCode)
                            return;
                    }
                    catch (Exception ex)
                    {

                    }

                    await Task.Delay(2000);
                }

                throw new Exception("API not ready in time.");
            }
            catch (Exception ex)
            {

            }
        }

        private async Task CreateWebApiContainerAsync()
        {
            try
            {
                var connString = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString())
                {
                    InitialCatalog = "GateEntryExit"
                }.ToString();

                _webApiContainer = new ContainerBuilder()
                                   .WithImage("gate-entry-exit-web-api-image")
                                   .WithName("gate-entry-exit-web-api-container")
                                   .WithNetwork("dockerNetwork")
                                   .WithEnvironment("ConnecitonStrings__Default", ReplaceServerName(connString, _msSqlContainer.Name, 1433))
                                   .WithEnvironment("ASPNETCORE_URLS", "http://0.0.0.0:5000")
                                   .WithEnvironment("ASPNETCORE_ENVIRONMENT", "true")
                                   .WithEnvironment("Use_DockerWay", "true")
                                   .WithExposedPort(5000)
                                   .WithPortBinding(5000, 5000)
                                   .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5000))
                                   .DependsOn(_msSqlContainer)
                                   .Build();
                await _webApiContainer.StartAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task RestoreDatabaseAsync()
        {
            try
            {
                var connString = new SqlConnection(_msSqlContainer.GetConnectionString());
                await connString.OpenAsync();

                var restoreCommands = new[]
                {
                    new
                    {
                        DbName = "Gate-Entry-Exit",
                        BakFile = "/var/opt/mssql/backup/GateEntryExit.bak",
                        LogicalData = "GateEntryExit",
                        LogicalLog = "GateEntryExit_log",
                        Mdf = "/var/opt/mssql/data/GateEntryExit.mdf",
                        Ldf = "/var/opt/mssql/data/GateEntryExit_log.ldf",
                        ValidationQuery = "Select Count(*) From dbo.GateEntries"
                    }
                };

                foreach (var db in restoreCommands)
                {
                    var restoreSql = $@"
                    RESTORE DATABASE [{db.DbName}]
                    FROM DISK = '{db.BakFile}'
                    WITH 
                        MOVE '{db.LogicalData}' TO '{db.Mdf}',
                        MOVE '{db.LogicalLog}' TO '{db.Ldf}',
                        REPLACE,
                        RECOVERY;";

                    using var restoreCmd = new SqlCommand(restoreSql, connString);
                    restoreCmd.CommandTimeout = 600;
                    await restoreCmd.ExecuteNonQueryAsync();

                    var dbConnStr = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString())
                    {
                        InitialCatalog = db.DbName
                    }.ToString();

                    using var dbconn = new SqlConnection(dbConnStr);
                    await dbconn.OpenAsync();

                    using var validationCmd = new SqlCommand(db.ValidationQuery, dbconn);
                    var result = (int)await validationCmd.ExecuteScalarAsync();

                    if(result == 0)
                    {
                        throw new Exception($"Restore passed but validation failed for database {db.DbName}.");
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private string ReplaceServerName(string connString, string host, int port)
        {
            var pattern = @"(Data Source|Server)=[^;]+;";
            var replacement = $"Data Souce={host.TrimStart('/')},{port};";
            return Regex.Replace(connString, pattern, replacement, RegexOptions.IgnoreCase);
        }

        private async Task ApplyPendingMigrationsAsync()
        {
            try
            {
                var connString = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString())
                {
                    InitialCatalog = "GateEntryExit"
                }.ToString();

                var options = new DbContextOptionsBuilder<GateEntryExitDbContext>()
                                .UseSqlServer(ReplaceServerName(connString, "localhost", 65473))
                                .Options;

                using var db = new GateEntryExitDbContext(options);

                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task CreateSqlContainerAsync()
        {
            try
            {
                //var dbBackupPath = Path.GetFullPath("../../src/DBBackup/GateEntryExit.bak");
                _msSqlContainer = new MsSqlBuilder()
                    .WithImage("gate-entry-exit-sql-server-image")
                    .WithPassword("Ohlord")
                    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433).AddCustomWaitStrategy(new WaitUnitlSqlServerImageReady()))
                    .WithName("gate-entry-exit-sql-server-container")
                    .WithNetwork("dockerNetwork")
                    .WithExposedPort(1433)
                    .WithPortBinding(65473, 1433)
                    .Build();
                _msSqlContainer.StartAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task CreateNetworkAsync()
        {
            try
            {
                var client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
                var networks = await client.Networks.ListNetworksAsync();

                if(!networks.Any(n => n.Name == "dockerNetwork"))
                {
                    await client.Networks.CreateNetworkAsync(new Docker.DotNet.Models.NetworksCreateParameters
                    {
                        Name = "dockerNetwork",
                        Driver = "bridge"
                    });
                }
            }
            catch(Exception ex)
            {

            }
        }

        public async Task DisposeAsync()
        {
            if (_playwrightContainer != null)
                _playwrightContainer.DisposeAsync();

            if (_webUiContainer != null)
                _webUiContainer.DisposeAsync();

            if (_webApiContainer != null)
                _webApiContainer.DisposeAsync();

            if (_msSqlContainer != null)
                _msSqlContainer.DisposeAsync();
        }
    }
}
