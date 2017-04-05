using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TcpLibrary.Common;
using TcpLibrary.Interface;
using TcpLibrary.Packet;
namespace TcpLibrary.Packet
{
    public delegate void ReceivePacketEventHandler<T>(MainPacket<T> packet) where T : struct; 
    public class PacketMaker<T> : IDisposable where T : struct
    {
        public event ReceivePacketEventHandler<T> ReceivePacket;
        int bufflong = 0;
        List<byte> buff = new List<byte>();
        /// <summary>
        /// 路由命令
        /// </summary>
        /// <param name="bytes">源比特流</param>
        public void Switch(byte[] bytes)
        {
            int pos = 0;
            if (bytes.Length == 0) return;
            if (bufflong < 0) { bufflong = 0; buff.Clear(); }    //重置掉无效的长度
            if (bytes.Length < 8) {
                buff.AddRange(bytes);
                if (buff.Count < 8)
                {
                    bufflong = 8 - buff.Count;
                    return;
                }
            }
            if (bufflong == 0 && bytes.Length >= 8)　　　 //如果没有还未接收完的数据
            {
                MainPacket<T> mp = new MainPacket<T>();
                pos += PacketHelper.GetPacketLen(Tools.SubBytes(bytes, pos));            //获取数据包长度
                if (pos > bytes.Length)                                                   //如果数据包长度大于缓冲区长度，说明包不完整，继续接收不解析
                {
                    bufflong = pos - bytes.Length;                                        //设置未接受数据长度为 数据包长度-当前缓冲区长度
                    buff.AddRange(bytes);                                                 //将数据存入缓冲区
                }
                else                                                                      //如果数据包长度小于等于缓冲区长度，说明是完整包
                {
                    PacketHelper.CreatePacketFromBytes(Tools.SubBytes(bytes, pos, 0), ref mp);   //解析包
                    ReceivePacket(mp);                               //整合成完整包，并触发事件
                    var temp = bufflong;                                                  //临时保存最后一段数据长度
                    bufflong = 0;                                                         //设置未接受数据长度为0
                    if (pos < bytes.Length) Switch(Tools.SubBytes(bytes, pos - temp));            //如果这个数据比这个包还要长，说明是下一个包的开始，继续解析

                }
            }
            else                                                                          //如果需要继续接收
            {
                if (bytes.Length < bufflong)                                              //如果收到的数据比未接收的数据短，说明还没收完，继续接收
                {
                    buff.AddRange(bytes);                                                 //数据存入缓冲区
                    bufflong = bufflong - bytes.Length;                                   //设置未接受数据长度为 当前缓冲区长度-数据包长度
                }
                else                                                                      //如果收到的数据长度大于等于未接收的数据，说明是完整包
                {
                    buff.AddRange(Tools.SubBytes(bytes, bufflong, 0));                           //将最后一段数据存入缓冲区
                    var temp = bufflong;                                                  //临时保存最后一段数据长度
                    bufflong = 0;                                                         //设置未接收的数据长度为0
                    Switch(buff.ToArray());                                                //开始解析此包
                    buff.Clear();                                                         //清空缓冲区
                    if (bytes.Length > temp) Switch(Tools.SubBytes(bytes, temp));                 //如果这段数据还有更多内容，说明是下一个包的开始，继续解析
                }
            }
        }

        public void Dispose()
        {
            buff.Clear();
        }
    }
    public static class PacketHelper
    {
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
