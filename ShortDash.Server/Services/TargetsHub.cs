using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShortDash.Server.Services
{
    public class TargetsHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"Received Message from {user}: {message}");
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
