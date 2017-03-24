using System;
using System.Collections.Generic;
using System.Text;
using TcpLibrary.Packet;

namespace TcpLibrary.Common
{
    /// <summary>
    /// 接收到的信息
    /// </summary>
    /// <typeparam name="T">命令列举类型</typeparam>
    public class Message<T>
    {
        /// <summary>
        /// 信息源
        /// </summary>
        public SimpleTcpClient<T> Socket { get; set; } = null;
        /// <summary>
        /// 信息正文
        /// </summary>
        public MainPacket<T> Packet { get; set; } = null;
    }
}
