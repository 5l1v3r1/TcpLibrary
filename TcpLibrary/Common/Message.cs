using System;
using System.Collections.Generic;
using System.Text;
using TcpLibrary.Packet;

namespace TcpLibrary.Common
{
    public class Message<T>
    {
        public SimpleTcpClient<T> Socket { get; set; } = null;
        public MainPacket<T> Packet { get; set; } = null;
    }
}
