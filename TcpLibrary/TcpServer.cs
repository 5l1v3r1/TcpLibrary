using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TcpLibrary.Common;
using TcpLibrary.Packet;

namespace TcpLibrary
{
    
    public class TcpServer<T> : TcpBase<T>, IDisposable where T : struct
    {
        public delegate void ClientComingEventHandler(SimpleTcpClient<T> sock);
        public delegate void ClientClosingEventHandler(SimpleTcpClient<T> sock);

        public event ClientComingEventHandler OnClientComing;
        public event ClientClosingEventHandler OnClientClosing;

        private TcpListener TcpListen = null;

        public List<SimpleTcpClient<T>> Clients = new List<SimpleTcpClient<T>>();

        public TcpServer(int port)
        {
            TcpListen = new TcpListener(IPAddress.Any, port);
            ObjectFactory.Init();
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
            stc.Disconnect += disconnect;
            stc.ReceivePacket += Switch;
            stc.ns = s2.GetStream();
            stc.StartRecv();
            Clients.Add(stc);
            OnClientComing?.Invoke(stc);
            s.BeginAcceptTcpClient(new AsyncCallback(Listen_Callback), s);
        }

        private void disconnect(object sender, string errmsg)
        {
            var stc = sender as SimpleTcpClient<T>;
            OnClientClosing?.Invoke(stc);
            Clients.Remove(stc);
        }

        public void Dispose()
        {
            Clients.Clear();
        }
    }
}
