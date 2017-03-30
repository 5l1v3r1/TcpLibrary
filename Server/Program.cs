using Client.Common;
using Client.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TcpLibrary;
using TcpLibrary.Common;
using TcpLibrary.Packet;

namespace Server
{
    class Program
    {
        static TcpServer<NetCommand> server = new TcpServer<NetCommand>(9999);
        static void Main(string[] args)
        {
            InitPacketFunc();
            server.OnClientComing += Server_OnClientComing;
            server.OnClientClosing += Server_OnClientClosing;
            server.Start();
            Console.ReadLine();
        }

        private static void Server_OnClientClosing(SimpleTcpClient<NetCommand> sock)
        {
            Console.WriteLine("exiting");
        }

        private static void Server_OnClientComing(SimpleTcpClient<NetCommand> sock)
        {
            Console.WriteLine("comeming");
        }

        public static void InitPacketFunc()
        {
            server.RegAction(NetCommand.Text, TextMessage);
        }
        public static void TextMessage(Message<NetCommand> message)
        {
            var packet = new TextPacket();
            PacketHelper.CreatePacketFromBytes(message.Packet.Data, ref packet);
            Console.WriteLine(packet.Text);
            message.Socket.Send(message.Packet);
        }
    }
}
