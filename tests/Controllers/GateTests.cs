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
    [Collection("Common Test Collection")]
    public class GateTests : IClassFixture<InProcessFactory>
    {
        private readonly ITestWay _testWay;
        private readonly GateEntryExitDbContext _gateEntryExitDBContext;
        private HttpClient _httpClient { get; }

        public GateTests(CommonFixture commonFixture)
        {
            _testWay = commonFixture._testWay;

            //_gateEntryExitDBContext = serviceProvider.GetRequiredService<GateEntryExitDbContext>();

            // Use this client to do(SendAsync) api call after creating HttpRequestMessage, read response from HttpResponseMessage
            _httpClient = _testWay.CreateClient();

            // To make sure appsettings.test.json db connection came here
            //var connectionString = _gateEntryExitDBContext.Database.GetConnectionString();
        }

        [Fact]
        public async Task Can_Create_GateEntry()
        {

        }
    }
}
