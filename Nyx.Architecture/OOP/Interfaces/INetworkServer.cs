using Nyx.Architecture.OOP.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nyx.Architecture.OOP.Interfaces
{
    public interface INetworkServer : IDisposable
    {
        string Name { get; }
        int Port { get; }
        ProtocolType Protocol { get; }
        bool IsRunning { get; }

        void Start();
        void Stop();
        void ForwardTo(INetworkServer target, TrafficRule rule);
        void ReceiveData(byte[] data, EndPoint remoteEndPoint, string clientId);

        event EventHandler<OOP.Handlers.DataReceivedEventArgs> DataReceived;
        event EventHandler<OOP.Handlers.ClientConnectedEventArgs> ClientConnected;
        event EventHandler<OOP.Handlers.ClientDisconnectedEventArgs> ClientDisconnected;
    }


}
