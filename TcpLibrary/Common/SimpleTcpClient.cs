using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TcpLibrary.Converter;
using TcpLibrary.Interface;
using TcpLibrary.Packet;

namespace TcpLibrary.Common
{
    public delegate void DisconnectEventHandler(object sender, string errmsg);
    public class SimpleTcpClient<T> : IDisposable
    {

        public delegate void ReceivePacketEventHandler(SimpleTcpClient<T> sender, MainPacket<T> packet);
        private PacketMaker<T> PM = new PacketMaker<T>();
        public TcpClient Socket = null;
        public NetworkStream Ns = null;
        public string Hostname = string.Empty;
        public int Port = 0;
        public event ReceivePacketEventHandler ReceivePacket;
        public event DisconnectEventHandler Disconnect;
        public object Tag = null;
        public IConvert Convert;
        public int BufferLength = 2048;
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
                Ns = Socket.GetStream();
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
            Tools.StartThread(new ThreadStart(Recv));
        }
        private void Recv()
        {
            while (true)
            {
                byte[] recvBytes = new byte[BufferLength];
                int bytes = 0;
                try
                {
                    bytes = Ns.Read(recvBytes, 0, BufferLength);
                }
                catch { }
                try
                {
                    if (bytes <= 0)
                    {
                        Disconnect?.Invoke(this, "Disconnect");
                        Debug.WriteLine("Recv Switch Error: byte length is 0, Disconnect");
                        break;
                    }
                    try
                    {
                        PM.Switch((Tools.SubBytes(recvBytes, bytes, 0)));
                    }
                    catch (Exception ex) { Debug.WriteLine("Recv Switch Error:" + ex.Message); }
                }
                catch (Exception ex)
                {
                    Disconnect?.Invoke(this, ex.ToString());
                    break;
                }

            }
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
            try
            {
                lock (Ns)
                {
                    Ns.Write(bytesSendData, 0, bytesSendData.Length);
                    Ns.Flush();
                }
            }
            catch (Exception ce)
            {
                Debug.WriteLine("Send Error:" + ce.Message);
            }
        }
        public void Dispose()
        {
            Socket?.Client.Shutdown(SocketShutdown.Both);
            Socket = null;
            PM.Dispose();
        }
    }
}
