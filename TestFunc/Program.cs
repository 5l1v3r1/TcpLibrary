using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TcpLibrary.Common;
using TcpLibrary.Packet;
using TcpLibrary.Interface;
namespace TestFunc
{
    class Program
    {
        static void Main(string[] args)
        {
            ObjectFactory.Init();
            byte[] sresult = null;


            int[] int_inttest = new int[] { 100000, 5556, 55964636, 54564, 2131, 0, 3333, 66, 644, 444 };
            sresult = ObjectFactory.ToBytes(typeof(int[]), int_inttest);
            int[] int_outtest = null;
            int_outtest = (int[])ObjectFactory.ToObjact(typeof(int[]), sresult);

            string[] str_inttest = new string[] { "asfdasfasfas", "阿斯蒂芬阿三豆腐块阿斯弗", "发哈可是大后方恐惧哈市", "啊开始就好付款计划v卡收费的卡上的机会附加费感觉哈是官方的环境啊是的高房价哈斯夺冠翻噶实践活动覆盖较好啊是固定火箭发射轨道" };
            sresult = ObjectFactory.ToBytes(typeof(string[]), str_inttest);
            string[] str_outtest = null;
            str_outtest = (string[])ObjectFactory.ToObjact(typeof(string[]), sresult);

            bool[] bool_inttest = new bool[] { true, false, true, false, true, true, true, true, false, false, false, false };
            sresult = ObjectFactory.ToBytes(typeof(bool[]), bool_inttest);


            MainPacket<cc> tp = new MainPacket<cc>();
            tp.CommandType = cc.v1;
            tp.Data = sresult;
            var bytes = ObjectFactory.ToBytes(tp.GetType(), tp);

            object obj = ObjectFactory.ToObjact(tp.GetType(), bytes);
            // tp1.CreatePacketFromBytes(tempb);
        }
    }
    public class TestPacket : PacketBase, IPacket
    {
        public int test1 { get; set; } = 0;
        public string str { get; set; } = string.Empty;

    }
    public enum cc
    {
        v1 = 1, v2 = 2
    }
}
