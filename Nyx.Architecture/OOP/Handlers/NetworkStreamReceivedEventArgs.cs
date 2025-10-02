using System.Net;
using System.Net.Sockets;

namespace Nyx.Architecture.OOP.Handlers
{
    public class NetworkStreamReceivedEventArgs
    {
        public NetworkStream Data { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
        public string ClientId { get; set; }
    }
}
