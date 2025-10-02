using Nyx.Architecture.OOP.Enums;
using Nyx.Architecture.OOP.Handlers;
using Nyx.Architecture.OOP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nyx.Architecture.OOP.Abstractions
{
    public abstract class BaseNetworkServer : INetworkServer
    {
        public string Name { get; protected set; }
        public int Port { get; protected set; }
        public ProtocolType Protocol { get; protected set; }
        public bool IsRunning { get; protected set; }

        protected readonly List<INetworkServer> _forwardTargets;
        protected readonly List<TrafficRule> _forwardingRules;
        protected CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        protected BaseNetworkServer(string name, int port, ProtocolType protocol)
        {
            Name = name;
            Port = port;
            Protocol = protocol;
            _forwardTargets = new List<INetworkServer>();
            _forwardingRules = new List<TrafficRule>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public abstract void Start();
        public abstract void Stop();

        public virtual void ForwardTo(INetworkServer target, TrafficRule rule)
        {
            if (!_forwardTargets.Contains(target))
            {
                _forwardTargets.Add(target);
                _forwardingRules.Add(rule);
            }
        }

        protected virtual void OnDataReceived(DataReceivedEventArgs e)
        {
            DataReceived?.Invoke(this, e);
            ForwardDataToTargets(e.Data, e.RemoteEndPoint);
        }

        protected virtual void OnClientConnected(ClientConnectedEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        protected virtual void OnClientDisconnected(ClientDisconnectedEventArgs e)
        {
            ClientDisconnected?.Invoke(this, e);
        }
        protected virtual void ForwardDataToTargets(byte[] data, EndPoint sourceEndPoint)
        {
            for (int i = 0; i < _forwardTargets.Count; i++)
            {
                var target = _forwardTargets[i];
                var rule = _forwardingRules[i];

                if (ShouldForward(data, rule))
                {
                    Task.Run(() => target.ReceiveData(
                        data,
                        sourceEndPoint,
                        $"ForwardedFrom_{Name}"
                    ));
                }
            }
        }

        // Implement the new method in BaseNetworkServer
        public virtual void ReceiveData(byte[] data, EndPoint remoteEndPoint, string clientId)
        {
            // This safely triggers the event from within the class
            OnDataReceived(new DataReceivedEventArgs
            {
                Data = data,
                RemoteEndPoint = remoteEndPoint,
                ClientId = clientId
            });
        }

        protected virtual bool ShouldForward(byte[] data, TrafficRule rule)
        {
            return rule switch
            {
                TrafficRule.All => true,
                TrafficRule.OnlyTcp => Protocol == ProtocolType.SocketTcp || Protocol == ProtocolType.TcpListener,
                TrafficRule.OnlyUdp => Protocol == ProtocolType.SocketUdp || Protocol == ProtocolType.UdpListener,
                _ => true
            };
        }

        public virtual void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }
    }
}
