using Nyx.Architecture.OOP.Abstractions;
using Nyx.Architecture.OOP.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nyx.Architecture.Network
{
    public class TcpListenerServer : BaseNetworkServer
    {
        private TcpListener _tcpListener;
        private readonly List<TcpClient> _connectedClients;

        public TcpListenerServer(string name, int port)
            : base(name, port, OOP.Enums.ProtocolType.TcpListener)
        {
            _connectedClients = new List<TcpClient>();
        }

        public override void Start()
        {
            if (IsRunning) return;

            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();
            IsRunning = true;

            Console.WriteLine($"{Name} (TCP Listener) started on port {Port}");

            // Start accepting clients
            Task.Run(AcceptClientsAsync);
        }

        private async Task AcceptClientsAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested && IsRunning)
            {
                try
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    _connectedClients.Add(tcpClient);

                    var clientId = Guid.NewGuid().ToString();
                    OnClientConnected(new ClientConnectedEventArgs
                    {
                        ClientId = clientId,
                        RemoteEndPoint = tcpClient.Client.RemoteEndPoint
                    });

                    // Start handling client
                   await Task.Run(() => HandleClientAsync(tcpClient, clientId));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient, string clientId)
        {
            var stream = tcpClient.GetStream();
            var buffer = new byte[4096];

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested && tcpClient.Connected)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                    if (bytesRead == 0) break;

                    var data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);

                    OnDataReceived(new DataReceivedEventArgs
                    {
                        Data = data,
                        RemoteEndPoint = tcpClient.Client.RemoteEndPoint,
                        ClientId = clientId
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client {clientId}: {ex.Message}");
            }
            finally
            {
                _connectedClients.Remove(tcpClient);
                tcpClient.Close();
                OnClientDisconnected(new ClientDisconnectedEventArgs
                {
                    ClientId = clientId,
                    RemoteEndPoint = tcpClient.Client.RemoteEndPoint
                });
            }
        }

        public override void Stop()
        {
            if (!IsRunning) return;

            IsRunning = false;
            _cancellationTokenSource.Cancel();

            _tcpListener?.Stop();

            foreach (var client in _connectedClients)
            {
                client.Close();
            }
            _connectedClients.Clear();

            Console.WriteLine($"{Name} (TCP Listener) stopped");
        }
    }
}
