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
using System.Threading;

namespace Client
{
    class Program
    {
        static Random random = new Random();
        static void Main(string[] args)
        {
            ObjectFactory.Init();
            int valid_connection = 0;
            for (int i = 0; i < 50000; i++)
            {
                Console.Title = valid_connection.ToString();
                new Thread(new ThreadStart(delegate
                {
                    TcpClient<NetCommand> client = new TcpClient<NetCommand>();
                    client.RegAction<TextPacket>(NetCommand.Text, TextMessage);
                    if (client.Connect(new IPEndPoint(IPAddress.Parse("192.168.11.113"), 9999)))
                    {
                        valid_connection++;
                        client.Send(NetCommand.Text, new TextPacket()
                        {
                            Text = "test",
                            Strs = new string[] {
                                "asdfhas",
                                "啊上雕刻技法哈萨克发货卡很疯狂啊伤口缝合伤口啊贺卡设计的哈收到回复卡仕达"
                            }
                        });
                    }
                    client.Disconnect += Client_Disconnect;
                })).Start();
            }



            Console.ReadLine();
        }

        private static void Client_Disconnect(object sender, string errmsg)
        {
            //Console.WriteLine("disconnect");
        }

        public static void TextMessage(Message<NetCommand, TextPacket> message)
        {
            Thread.Sleep(random.Next(1000));
            message.Socket.Send(new MainPacket<NetCommand>(NetCommand.Text, message.Packet));
        }
    }
}
