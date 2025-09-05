using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateEntryExit.Test.Controllers
{
    public class CommonFixture : IAsyncLifetime
    {
        public ITestWay _testWay;

        public CommonFixture()
        {
            _testWay = TestWayFactory.Create();
        }

        public async Task InitializeAsync() => await _testWay.InitializeAsync();

        public async Task DisposeAsync() => await _testWay.DisposeAsync();
    }
}
