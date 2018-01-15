using System;
using System.Collections.Generic;
using System.Text;
using TcpLibrary.Common;
using TcpLibrary.Interface;
using TcpLibrary.Packet;
using System.Reflection;
namespace TcpLibrary
{
    public class TcpBase<T> where T : struct
    {
        private Dictionary<T, Router> CommandRouter = new Dictionary<T, Router>();
        public void RegAction<P>(T type, Action<Message<T, P>> func) where P : PacketBase, IPacket
        {
            if (CommandRouter.ContainsKey(type))
                CommandRouter.Remove(type);
            CommandRouter.Add(type, new Router { Action = func, DefaultType = typeof(P) });
        }
        public void UnRegAction(T type)
        {
            if (CommandRouter.ContainsKey(type))
                CommandRouter.Remove(type);
        }

        public void Switch(SimpleTcpClient<T> sender, MainPacket<T> packet)
        {
            if (CommandRouter.ContainsKey(packet.CommandType))
            {
                var router = CommandRouter[packet.CommandType];
                var ptype = router.DefaultType;
                var _packet = ObjectFactory.ToObjact(ptype, packet.Data);
                var methodInfo = router.Action.GetType().GetMethod("Invoke");
                var paramInfo = methodInfo.GetParameters()[0].ParameterType;
                
                var message = Activator.CreateInstance(paramInfo);
                var message_type = message.GetType();
                message_type.GetField("Socket").SetValue(message, sender);
                message_type.GetField("Packet").SetValue(message, _packet);
                methodInfo.Invoke(router.Action, new object[] { message });
            }
        }
    }
}