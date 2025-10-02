using Nyx.Architecture.Core;
using Nyx.Architecture.OOP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nyx.Architecture
{
    internal class Main
    {
        static async Task MainFunction(string[] args)
        {
            // Create servers using factory
            var tcpServer = NetworkServerFactory.CreateServer("MainTCP", 8080, ProtocolType.TcpListener);
            var udpServer = NetworkServerFactory.CreateServer("MainUDP", 8081, ProtocolType.UdpListener);
            var socketTcpServer = NetworkServerFactory.CreateServer("SocketTCP", 8082, ProtocolType.SocketTcp);

            // Setup event handlers
            tcpServer.DataReceived += (sender, e) =>
                Console.WriteLine($"[TCP] Received {e.Data.Length} bytes from {e.ClientId}");

            udpServer.DataReceived += (sender, e) =>
                Console.WriteLine($"[UDP] Received {e.Data.Length} bytes from {e.RemoteEndPoint}");

            socketTcpServer.DataReceived += (sender, e) =>
                Console.WriteLine($"[SocketTCP] Received {e.Data.Length} bytes from {e.ClientId}");

            // Setup forwarding: TCP -> UDP and TCP -> SocketTCP
            tcpServer.ForwardTo(udpServer, TrafficRule.All);
            tcpServer.ForwardTo(socketTcpServer, TrafficRule.OnlyTcp);

            // Start all servers
            tcpServer.Start();
            udpServer.Start();
            socketTcpServer.Start();

            Console.WriteLine("All servers started. Press any key to stop...");
            Console.ReadKey();

            // Cleanup
            tcpServer.Stop();
            udpServer.Stop();
            socketTcpServer.Stop();

            tcpServer.Dispose();
            udpServer.Dispose();
            socketTcpServer.Dispose();
        }
    }
}
