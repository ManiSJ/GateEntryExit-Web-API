using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateEntryExit.Test.Controllers
{
    public static class TestWayFactory
    {
        public static ITestWay Create()
        {
            var isDockerWay = Environment.GetEnvironmentVariable("DockerWay")?.ToLower() == "true";

            return isDockerWay ? new DockerWay() : new InProcessWay();
        }
    }
}
