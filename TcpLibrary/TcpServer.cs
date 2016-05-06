﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TcpLibrary.Common;
using TcpLibrary.Packet;

namespace TcpLibrary
{
    
    public class TcpServer<T>
    {
        public delegate void ClientComingEventHandler(SimpleTcpClient<T> sock);
        public delegate void ClientClosingEventHandler(SimpleTcpClient<T> sock);
        public delegate void MessageComingEventHandler(SimpleTcpClient<T> sock, MainPacket<T> packet);

        public event ClientComingEventHandler OnClientComing;
        public event ClientClosingEventHandler OnClientClosing;
        public event MessageComingEventHandler OnMessageComing;

        private TcpListener TcpListen = null;

        public List<SimpleTcpClient<T>> Clients = new List<SimpleTcpClient<T>>();

        public TcpServer(int port)
        {
            TcpListen = new TcpListener(IPAddress.Any, port);
        }
        public void Start()
        {
            TcpListen.Start(TcpConfig.ServerMaxClient);
            TcpListen.BeginAcceptTcpClient(new AsyncCallback(Listen_Callback), TcpListen);
        }
        public void Stop()
        {
            TcpListen.Stop();
        }
        private void Listen_Callback(IAsyncResult ar)
        {
            TcpListener s = (TcpListener)ar.AsyncState;
            TcpClient s2 = s.EndAcceptTcpClient(ar);
            SimpleTcpClient<T> stc = new SimpleTcpClient<T>();
            stc.Socket = s2;
            stc.Disconnect += Stc_Disconnect;
            stc.ReceivePacket += Stc_ReceivePacket;
            stc.Ns = s2.GetStream();
            stc.StartRecv();
            Clients.Add(stc);
            OnClientComing?.Invoke(stc);
            s.BeginAcceptTcpClient(new AsyncCallback(Listen_Callback), s);
        }

        private void Stc_Disconnect(object sender, string errmsg)
        {
            var stc = sender as SimpleTcpClient<T>;
            Clients.Remove(stc);
            OnClientClosing?.Invoke(stc);
        }

        private void Stc_ReceivePacket(SimpleTcpClient<T> sender, MainPacket<T> packet)
        {
            OnMessageComing(sender, packet);
        }
    }
}