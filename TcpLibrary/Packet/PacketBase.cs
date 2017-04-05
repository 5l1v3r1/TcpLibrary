using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TcpLibrary.Common;
using TcpLibrary.Interface;
using TcpLibrary.Packet;
namespace TcpLibrary.Packet
{
    public delegate void ReceivePacketEventHandler<T>(MainPacket<T> packet) where T : struct;
    public class PacketMaker<T> where T : struct
    {
        public event ReceivePacketEventHandler<T> ReceivePacket;
        private byte[] readData(Stream s, int length)
        {
            byte[] data = new byte[length];
            int recvlength = 0;
            while (recvlength != length)
                recvlength += s.Read(data, recvlength, length - recvlength);
            return data;
        }
        /// <summary>
        /// 路由命令
        /// </summary>
        /// <param name="bytes">源比特流</param>
        public void Switch(Stream s)
        {
            byte[] header = readData(s, PacketHelper.HeaderLength);
            var datalength = PacketHelper.GetPacketLen(header);
            byte[] data = readData(s, datalength - PacketHelper.HeaderLength);
            var packet = new byte[header.Length + data.Length];
            Buffer.BlockCopy(header, 0, packet, 0, header.Length);
            Buffer.BlockCopy(data, 0, packet, header.Length, data.Length);
            MainPacket<T> mp = new MainPacket<T>();
            PacketHelper.CreatePacketFromBytes(packet, ref mp);
            ReceivePacket(mp);
        }
    }
    public static class PacketHelper
    {
        public static int HeaderLength = 8;
        public static int GetPacketLen(byte[] bytes)
        {
            return 8 + BitConverter.ToInt32(bytes, 4);
        }

        /// <summary>
        /// 从比特流解析出数据包
        /// </summary>
        /// <typeparam name="T">数据包类型</typeparam>
        /// <returns>数据包长度</returns>
        /// <param name="bytes">比特流</param>
        /// <param name="p">返回数据包</param>
        public static void CreatePacketFromBytes<T>(byte[] bytes, ref T p) where T : PacketBase, IPacket
        {
            p = (T)ObjectFactory.ToObjact(p.GetType(), bytes, p._Encoding.HeaderName);
        }
    }
    public class PacketBase
    {
        /// <summary>
        /// 设置数据包编解码使用的字符编码类型
        /// </summary>
        public Encoding _Encoding = Encoding.ASCII;

        /// <summary>
        /// 获取数据包比特流
        /// </summary>
        /// <returns>比特流</returns>
        public byte[] GetBytes()
        {
            return ObjectFactory.ToBytes(this.GetType(), this, _Encoding.HeaderName);
        }
    }
}
