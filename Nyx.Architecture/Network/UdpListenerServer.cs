using Nyx.Architecture.OOP.Abstractions;
using Nyx.Architecture.OOP.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nyx.Architecture.Network
{
    public class UdpListenerServer : BaseNetworkServer
    {
        private UdpClient _udpClient;

        public UdpListenerServer(string name, int port)
            : base(name, port, OOP.Enums.ProtocolType.UdpListener)
        {
        }

        public override void Start()
        {
            if (IsRunning) return;

            _udpClient = new UdpClient(Port);
            IsRunning = true;

            Console.WriteLine($"{Name} (UDP Listener) started on port {Port}");

            // Start receiving data
            Task.Run(ReceiveDataAsync);
        }

        private async Task ReceiveDataAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested && IsRunning)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();
                    var data = result.Buffer;

                    OnDataReceived(new DataReceivedEventArgs
                    {
                        Data = data,
                        RemoteEndPoint = result.RemoteEndPoint,
                        ClientId = $"UDP_{result.RemoteEndPoint}"
                    });
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving UDP data: {ex.Message}");
                }
            }
        }

        public override void Stop()
        {
            if (!IsRunning) return;

            IsRunning = false;
            _cancellationTokenSource.Cancel();
            _udpClient?.Close();

            Console.WriteLine($"{Name} (UDP Listener) stopped");
        }
    }
}
