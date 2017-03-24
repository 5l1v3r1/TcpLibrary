using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TcpLibrary;
using Client.Packet;
using Client.Common;
using TcpLibrary.Common;
using TcpLibrary.Packet;
using System.Net;
namespace Client
{
    class Program
    {
        static TcpClient<NetCommand> client = new TcpClient<NetCommand>();
        static void Main(string[] args)
        {
            InitPacketFunc();
            client.Connect(new IPEndPoint(IPAddress.Loopback, 9999));
            client.Send(new MainPacket<NetCommand>(NetCommand.Text, new TextPacket() { Text = "test" }));
            Console.ReadLine();
        }
        public static void InitPacketFunc()
        {
            client.RegAction(NetCommand.Text, TextMessage);
        }
        public static void TextMessage(Message<NetCommand> message)
        {
            var packet = new TextPacket();
            PacketHelper.CreatePacketFromBytes(message.Packet.Data, ref packet);
            Console.WriteLine(packet.Text);
        }
    }
}
