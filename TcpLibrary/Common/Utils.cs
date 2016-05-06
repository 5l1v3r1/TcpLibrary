using System;
using System.Text;
using System.Threading;

namespace TcpLibrary.Common
{
    class Tools
    {
        public static byte[] SubBytes(byte[] bytes, int temp)
        {
            var databytes = new byte[bytes.Length - temp];
            Buffer.BlockCopy(bytes, temp, databytes, 0, bytes.Length - temp);
            return databytes;
        }
        public static byte[] SubBytes(byte[] bytes, int len, int temp)
        {
            var databytes = new byte[len];
            Buffer.BlockCopy(bytes, temp, databytes, 0, len);
            return databytes;
        }
        public static byte[] SubBytes(byte[] bytes, int len, ref int temp)
        {
            var databytes = SubBytes(bytes, len, temp);
            temp += len;
            return databytes;
        }
        public static string ToString(byte[] bytes, int len, ref int temp, Encoding enc = null)
        {
            return ToRealString(SubBytes(bytes, len, ref temp), enc);
        }
        public static int ToInt32(byte[] bytes, ref int temp)
        {
            var val = BitConverter.ToInt32(bytes, temp);
            temp += 4;
            return val;
        }
        public static long ToLong(byte[] bytes, ref int temp)
        {
            var val = BitConverter.ToInt64(bytes, temp);
            temp += 8;
            return val;
        }
        public static string ToRealString(byte[] bytes, Encoding enc = null)
        {
            return enc == null ? Encoding.ASCII.GetString(bytes) : enc.GetString(bytes);
        }
        public static byte[] ToBytes(string str, Encoding enc = null)
        {
            return enc == null ? Encoding.ASCII.GetBytes(str) : enc.GetBytes(str);
        }
        public static void StartThread(ParameterizedThreadStart nded, object obj)
        {
            Thread th = new Thread(nded);
            th.IsBackground = true;
            th.Start(obj);
        }
        public static void StartThread(ThreadStart nded)
        {
            Thread th = new Thread(nded);
            th.IsBackground = true;
            th.TrySetApartmentState(ApartmentState.STA);
            th.Start();
        }
    }
}
