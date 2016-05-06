﻿using TcpLibrary.Interface;
using System;
using System.Net;
using TcpLibrary.Common;
using TcpLibrary.Packet;
using TcpLibrary.Converter;

namespace TcpLibrary
{

    public delegate void DisconnectEventHandler(object sender, string errmsg);

    public class TcpClient<T> : IDisposable
    {
        public delegate void MessageEventHandler(object sender, T commandType, byte[] snp);
        

        public event DisconnectEventHandler Disconnect;
        public event DisconnectEventHandler Connected;
        public event MessageEventHandler Message;

        SimpleTcpClient<T> Client = null;
        public string Name;
        public string ServerAddr = string.Empty;
        public int ServerPort = 0;
        public IPEndPoint ServerInfo()
        {
            return new IPEndPoint(IPAddress.Parse((ServerAddr)), ServerPort);
        }
        /// <summary>
        /// 发起连接
        /// </summary>
        /// <param name="iep"></param>
        /// <returns></returns>
        public bool Connect(IPEndPoint iep)
        {
            return Connect(iep.Address.ToString(), iep.Port);
        }

        public bool Connect(string hostName, int port)
        {
            if (Client != null)
            {
                Client.Dispose();
            }
            ServerAddr = hostName;
            ServerPort = port;
            Client = new SimpleTcpClient<T>(hostName, port);
            Client.ReceivePacket += Swith;
            Client.Disconnect += Client_Disconnect;
            if (Client.Connect())
            {
                Connected?.Invoke(this, "OK");
                return true;
            }
            else return false;
        }

        private void Client_Disconnect(object sender, string errmsg)
        {
            Disconnect?.Invoke(this, errmsg);
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="packet"></param>
        public void Send(MainPacket<T> packet)
        {
            Client.Send(packet);
        }
        private void Swith(SimpleTcpClient<T> sender, MainPacket<T> packet)
        {
            try
            {
                Message?.Invoke(this, packet.CommandType, packet.Data);
            }
            catch (Exception ex) { }
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
