using Microsoft.AspNetCore.Http;
using System.Net;

namespace GateEntryExit.Test.Controllers
{
    public class FakeIpMiddleware
    {
        private readonly RequestDelegate _next;

        public FakeIpMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            await _next(context);
        }
    }
}
