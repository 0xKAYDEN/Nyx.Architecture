using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nyx.Architecture.OOP.Handlers
{
    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
        public string ClientId { get; set; }
    }
}
