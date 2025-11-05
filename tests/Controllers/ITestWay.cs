using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateEntryExit.Test.Controllers
{
    public interface ITestWay
    {
        HttpClient CreateClient();

        Task InitializeAsync();

        Task DisposeAsync();
    }
}
