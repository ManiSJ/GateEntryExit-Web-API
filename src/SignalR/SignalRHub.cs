using Microsoft.AspNetCore.SignalR;

namespace GateEntryExit.SignalR
{
    public class SignalRHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.UserIdentifier;

            Console.WriteLine($"SingalR connection Id {connectionId} and userId {userId}");

            await base.OnConnectedAsync();
        }
    }
}
