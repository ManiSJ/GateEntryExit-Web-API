using GateEntryExit.Domain.Policy;
using GateEntryExit.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace GateEntryExit.Domain.Manager
{
    public class GateManager : IGateManager
    {
        private readonly IGateNameUniquePolicy _gateNameUniquePolicy;
        private readonly IHubContext<SignalRHub> _signalRHubContext;

        public GateManager(IGateNameUniquePolicy gateNameUniquePolicy,
            IHubContext<SignalRHub> signalRHubContext)
        {
            _gateNameUniquePolicy = gateNameUniquePolicy;
            _signalRHubContext = signalRHubContext;
        }

        public async Task<Gate> CreateAsync(Guid id, string name)
        {
            var isNameUnique = await _gateNameUniquePolicy.IsNameUniqueAsync(name);

            if (!isNameUnique)
                throw new Exception("Name is not unique");
            
            // For only to logged in userId
            await _signalRHubContext.Clients.User("loginUserId").SendAsync("GateCreated");
            // For all clients
            await _signalRHubContext.Clients.All.SendAsync("GateCreated");

            return new Gate(id, name);
        }
    }
}
