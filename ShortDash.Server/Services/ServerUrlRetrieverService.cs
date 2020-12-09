using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ShortDash.Server.Services
{
    public class ServerUrlRetrieverService
    {
        public ServerUrlRetrieverService(IServer server)
        {
            GenerateUrls(server);
        }

        public string[] Urls { get; private set; }

        private void GenerateUrls(IServer server)
        {
            var output = new List<string>();
            var addresses = server.Features?.Get<IServerAddressesFeature>()?.Addresses;
            foreach (var address in addresses)
            {
                var uri = new Uri(address);
                if (uri.Host != "[::]")
                {
                    output.Add(address);
                    continue;
                }
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces().Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback && n.OperationalStatus == OperationalStatus.Up);
                foreach (var networkInterface in networkInterfaces)
                {
                    var ipProperties = networkInterface.GetIPProperties();
                    if (networkInterface.OperationalStatus != OperationalStatus.Up || ipProperties.GatewayAddresses.Count == 0)
                    {
                        continue;
                    }
                    var unicastAddresses = ipProperties.UnicastAddresses.Where(u =>
                        (u.Address.AddressFamily == AddressFamily.InterNetwork && !ipProperties.GetIPv4Properties().IsAutomaticPrivateAddressingActive) ||
                        (u.Address.AddressFamily == AddressFamily.InterNetworkV6 && !u.Address.IsIPv6LinkLocal));
                    foreach (var unicastAddress in unicastAddresses)
                    {
                        output.Add($"{uri.Scheme}://{unicastAddress.Address}:{uri.Port}");
                    }
                }
            }
            Urls = output.ToArray();
        }
    }
}
