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
            client.Send(new MainPacket<NetCommand>(NetCommand.none, new TextPacket() { Text = "test",Strs = new string[] { "asdfhas","啊上雕刻技法哈萨克发货卡很疯狂啊伤口缝合伤口啊贺卡设计的哈收到回复卡仕达"} }));
            client.Disconnect += Client_Disconnect;
            Console.ReadLine();
        }

        private static void Client_Disconnect(object sender, string errmsg)
        {
            Console.WriteLine("disconnect");
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
