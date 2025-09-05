using GateEntryExit.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateEntryExit.Test.Controllers
{
    public class GateTests : IClassFixture<TestAppFactory>
    {
        private readonly TestAppFactory _factory;
        private readonly GateEntryExitDbContext _gateEntryExitDBContext;
        private HttpClient _httpClient { get; }

        public GateTests(TestAppFactory factory)
        {
            _factory = factory;
            var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            _gateEntryExitDBContext = serviceProvider.GetRequiredService<GateEntryExitDbContext>();

            // Use this client to do(SendAsync) api call after creating HttpRequestMessage, read response from HttpResponseMessage
            _httpClient = _factory.CreateClient();

            // To make sure appsettings.test.json db connection came here
            var connectionString = _gateEntryExitDBContext.Database.GetConnectionString();
        }

        [Fact]
        public async Task Can_Create_GateEntry()
        {

        }
    }
}
