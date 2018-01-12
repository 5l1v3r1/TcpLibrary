using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TcpLibrary.Converter;
using TcpLibrary.Interface;
using TcpLibrary.Packet;

namespace TcpLibrary.Common
{
    public delegate void DisconnectEventHandler(object sender, string errmsg);
    public class SimpleTcpClient<T> : IDisposable where T : struct
    {
        public delegate void ReceivePacketEventHandler(SimpleTcpClient<T> sender, MainPacket<T> packet);

        private PacketMaker<T> PM = new PacketMaker<T>();
        public TcpClient Socket = null;
        public NetworkStream ns = null;
        public string Hostname = string.Empty;
        public int Port = 0;
        public event ReceivePacketEventHandler ReceivePacket;
        public event DisconnectEventHandler Disconnect;
        public int BufferLength = 2048;

        /// <summary>
        /// 附加数据
        /// </summary>
        public object Tag { get; set; } = null;
        public IConvert Convert;

        public SimpleTcpClient() : this(TcpConfig.Convert) { }
        public SimpleTcpClient(IConvert convert) : this(string.Empty, 0, convert) { }
        public SimpleTcpClient(string host, int port, IConvert convert = null)
        {
            Hostname = host;
            Port = port;
            if (convert == null)
                Convert = TcpConfig.Convert;
            else Convert = convert;
            PM.ReceivePacket += PM_ReceivePacket;
            Socket = new TcpClient();
        }
        public bool Connect()
        {
            try
            {
                Socket.Connect(Hostname, Port);
                ns = Socket.GetStream();
                StartRecv();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public void StartRecv()
        {
            Tools.StartThread(new ThreadStart(delegate ()
            {
                try
                {
                    do
                    {
                        if (Socket.Connected)
                            PM.Switch(ns);
                    } while (true);
                }
                catch (Exception ex)
                {
                    Disconnect(this, "Recv Error");
#if DEBUG
                    Console.WriteLine(ex);
#endif
                }
            }));
        }
        private void PM_ReceivePacket(MainPacket<T> packet)
        {
            packet.Data = Convert.Decode(packet.Data);
            ReceivePacket?.Invoke(this, packet);
        }
        public void Send(MainPacket<T> packet)
        {
            packet.Data = Convert.Encode(packet.Data);
            var bytesSendData = packet.GetBytes();
            if (ns == null) throw new Exception("is not connected.");
            if (!ns.CanWrite) throw new Exception("stream can't write!");
            lock (ns)
            {
                ns.Write(bytesSendData, 0, bytesSendData.Length);
                ns.Flush();
            }
        }
        public void Dispose()
        {
            if (Socket?.Client.Connected == true)
                Socket?.Client.Shutdown(SocketShutdown.Both);
            Socket = null;
        }
    }
}
