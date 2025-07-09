using Microsoft.AspNetCore.SignalR;

namespace LearningManagementSystem.Extensions
{
    public class LMSHubb : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("ReceiveSystemMessage", "You are connected!");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("ReceiveSystemMessage", "A user has disconnected");
        }
    }
}
