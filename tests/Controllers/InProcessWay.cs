using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateEntryExit.Test.Controllers
{
    public class InProcessWay : TestWay, ITestWay
    {
        private readonly InProcessFactory _factory;

        public InProcessWay()
        {
            _factory = new InProcessFactory();
        }

        public HttpClient CreateClient() => _factory.CreateClient();

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync()
        {
            _factory.Dispose();
            return Task.CompletedTask;
        }
    }
}
