using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TcpLibrary.Common;
using TcpLibrary.Interface;
using TcpLibrary.Packet;

namespace TcpLibrary.Packet
{
    public delegate void ReceivePacketEventHandler<T>(MainPacket<T> packet);
    public class PacketMaker<T> : IDisposable
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
            if (bytes.Length < 8 && bufflong > 8) { buff.AddRange(bytes); bufflong = 8 - bytes.Length; return; }
            if (bufflong == 0 && bytes.Length >= 8)　　　 //如果没有还未接收完的数据
            {
                MainPacket<T> mp = new MainPacket<T>();
                pos += PacketHelper.GetPacketLen(Tools.SubBytes(bytes, pos), ref mp);            //获取数据包长度
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

        public static int GetPacketLen<T>(byte[] bytes, ref T p)
        {
            Type type = p.GetType();
            int seek = 0;
            int result = 0;
            Dictionary<FieldInfo, int> sizeList = new Dictionary<FieldInfo, int>();

            foreach (FieldInfo fi in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (fi.Name.StartsWith("_")) continue;
                switch (fi.FieldType.Name)
                {
                    case "Int32":
                    case "NetCommand":
                        seek += 4;
                        result += 4;
                        break;
                    case "Int64":
                        seek += 8;
                        result += 8;
                        break;
                    default:
                        sizeList.Add(fi, Tools.ToInt32(bytes, ref seek));
                        result += 4;
                        break;
                }
            }
            foreach (var item in sizeList)
            {
                result += item.Value;
            }
            return result;
        }

        /// <summary>
        /// 从比特流解析出数据包
        /// </summary>
        /// <typeparam name="T">数据包类型</typeparam>
        /// <returns>数据包长度</returns>
        /// <param name="bytes">比特流</param>
        /// <param name="p">返回数据包</param>
        public static int CreatePacketFromBytes<T>(byte[] bytes, ref T p)
        {
            Type type = p.GetType();
            int seek = 0;
            int result = 0;
            Dictionary<FieldInfo, int> sizeList = new Dictionary<FieldInfo, int>();
            foreach (FieldInfo fi in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (fi.Name.StartsWith("_")) continue;
                switch (fi.FieldType.Name)
                {
                    case "Int32":
                    case "NetCommand":
                        fi.SetValue(p, Tools.ToInt32(bytes, ref seek));
                        result += 4;
                        break;
                    case "Int64":
                        fi.SetValue(p, Tools.ToLong(bytes, ref seek));
                        result += 8;
                        break;
                    default:
                        sizeList.Add(fi, Tools.ToInt32(bytes, ref seek));
                        result += 4;
                        break;
                }
            }
            foreach (var item in sizeList)
            {
                result += item.Value;
                var fi = item.Key;
                if (item.Value == 0) continue;
                switch (fi.FieldType.Name)
                {
                    case "Byte[]":
                        fi.SetValue(p, Tools.SubBytes(bytes, item.Value, ref seek));
                        break;
                    case "String":
                        fi.SetValue(p,
                            Tools.ToString(
                                    bytes,
                                    item.Value,
                                    ref seek,
                                    (Encoding)type.GetField("_Encoding", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(p))
                                );
                        break;
                    default:
                        if (fi.FieldType.Name.Contains("Packet"))
                        {
                            var c = fi.GetValue(p);
                            CreatePacketFromBytes<Object>(Tools.SubBytes(bytes, item.Value, ref seek), ref c);
                        }
                        break;
                }
            }
            return result;
        }
    }
    public class PacketBase
    {
        public Encoding _Encoding = Encoding.ASCII;
        public PacketBase() { }
        /// <summary>
        /// 获取数据包比特流
        /// </summary>
        /// <returns>比特流</returns>
        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            List<byte> Data = new List<byte>();
            Type type = this.GetType();
            foreach (FieldInfo fi in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (fi.Name.StartsWith("_")) continue;
                switch (fi.FieldType.Name)
                {
                    case "String":
                        var tempstrbytes = Tools.ToBytes(fi.GetValue(this).ToString(), _Encoding);
                        bytes.AddRange(BitConverter.GetBytes(tempstrbytes.Length));
                        if (tempstrbytes.Length > 0) Data.AddRange(tempstrbytes);
                        break;
                    case "Int32":
                    case "NetCommand":
                        var tempint = (int)fi.GetValue(this);
                        bytes.AddRange(BitConverter.GetBytes(tempint));
                        break;
                    case "Byte":
                        var tempbytebytes = (byte)fi.GetValue(this);
                        bytes.Add(tempbytebytes);
                        break;
                    case "Int64":
                        var templong = (long)fi.GetValue(this);
                        bytes.AddRange(BitConverter.GetBytes(templong));
                        break;
                    default:
                        var tempbytesbytes = new byte[] { };
                        if (fi.FieldType.Name == "Byte[]")
                            tempbytesbytes = (byte[])fi.GetValue(this);
                        else if (fi.FieldType.Name.Contains("Packet"))
                            tempbytesbytes = ((IPacket)fi.GetValue(this)).GetBytes();
                        bytes.AddRange(BitConverter.GetBytes(tempbytesbytes.Length));
                        if (tempbytesbytes.Length > 0) Data.AddRange(tempbytesbytes);
                        break;
                }
            }
            bytes.AddRange(Data);
            return bytes.ToArray();
        }
    }
}
