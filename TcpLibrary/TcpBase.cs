using System;
using System.Collections.Generic;
using System.Text;
using TcpLibrary.Common;
using TcpLibrary.Packet;

namespace TcpLibrary
{
    public class TcpBase<T>
    {
        private Dictionary<T, Action<Message<T>>> CommandRouter = new Dictionary<T, Action<Message<T>>>();
        public void RegAction(T type, Action<Message<T>> func)
        {
            if (CommandRouter.ContainsKey(type))
                CommandRouter.Remove(type);
            CommandRouter.Add(type, func);
        }
        public void UnRegAction(T type)
        {
            if (CommandRouter.ContainsKey(type))
                CommandRouter.Remove(type);
        }
        public void Swith(SimpleTcpClient<T> sender, MainPacket<T> packet)
        {
            if (CommandRouter.ContainsKey(packet.CommandType))
                CommandRouter[packet.CommandType](new Message<T>() {  Packet = packet, Socket = sender});
        }
    }
}