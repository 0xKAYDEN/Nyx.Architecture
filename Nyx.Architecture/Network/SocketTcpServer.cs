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
    public class SocketTcpServer : BaseNetworkServer
    {
        private Socket _listenerSocket;
        private readonly List<Socket> _connectedClients;

        public SocketTcpServer(string name, int port)
            : base(name, port, OOP.Enums.ProtocolType.SocketTcp)
        {
            _connectedClients = new List<Socket>();
        }

        public override void Start()
        {
            if (IsRunning) return;

            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            _listenerSocket.Listen(10);
            IsRunning = true;

            Console.WriteLine($"{Name} (Socket TCP) started on port {Port}");

            Task.Run(AcceptClientsAsync);
        }

        private async Task AcceptClientsAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested && IsRunning)
            {
                try
                {
                    var clientSocket = await _listenerSocket.AcceptAsync();
                    _connectedClients.Add(clientSocket);

                    var clientId = Guid.NewGuid().ToString();
                    OnClientConnected(new ClientConnectedEventArgs
                    {
                        ClientId = clientId,
                        RemoteEndPoint = clientSocket.RemoteEndPoint
                    });

                    Task.Run(() => HandleClientAsync(clientSocket, clientId));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

        private async Task HandleClientAsync(Socket clientSocket, string clientId)
        {
            var buffer = new byte[4096];

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested && clientSocket.Connected)
                {
                    var bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (bytesRead == 0) break;

                    var data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);

                    OnDataReceived(new DataReceivedEventArgs
                    {
                        Data = data,
                        RemoteEndPoint = clientSocket.RemoteEndPoint,
                        ClientId = clientId
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling socket client {clientId}: {ex.Message}");
            }
            finally
            {
                _connectedClients.Remove(clientSocket);
                clientSocket.Close();
                OnClientDisconnected(new ClientDisconnectedEventArgs
                {
                    ClientId = clientId,
                    RemoteEndPoint = clientSocket.RemoteEndPoint
                });
            }
        }

        public override void Stop()
        {
            if (!IsRunning) return;

            IsRunning = false;
            _cancellationTokenSource.Cancel();

            _listenerSocket?.Close();

            foreach (var client in _connectedClients)
            {
                client.Close();
            }
            _connectedClients.Clear();

            Console.WriteLine($"{Name} (Socket TCP) stopped");
        }
    }
}
