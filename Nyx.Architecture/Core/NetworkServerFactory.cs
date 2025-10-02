using Nyx.Architecture.Network;
using Nyx.Architecture.OOP.Enums;
using Nyx.Architecture.OOP.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nyx.Architecture.Core
{
    public static class NetworkServerFactory
    {
        public static INetworkServer CreateServer(string name, int port, ProtocolType protocol)
        {
            return protocol switch
            {
                ProtocolType.TcpListener => new TcpListenerServer(name, port),
                ProtocolType.UdpListener => new UdpListenerServer(name, port),
                ProtocolType.SocketTcp => new SocketTcpServer(name, port),
                ProtocolType.SocketUdp => throw new NotImplementedException("Socket UDP not implemented in MVP"),
                ProtocolType.Socket => throw new NotImplementedException("Raw Socket not implemented in MVP"),
                _ => throw new ArgumentException($"Unsupported protocol: {protocol}")
            };
        }
    }
}
