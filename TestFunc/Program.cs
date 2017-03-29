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
            ObjectFactory of = new ObjectFactory();
            var len = 0;
            byte[] sresult = null;


            int[] int_inttest = new int[] { 100000, 5556, 55964636, 54564, 2131, 0, 3333, 66, 644, 444 };
            sresult = of.ToBytes(typeof(int[]), int_inttest, out len);
            int[] int_outtest = null;
            int_outtest = (int[])of.ToObjact(typeof(int[]), sresult);

            string[] str_inttest = new string[] { "asfdasfasfas", "阿斯蒂芬阿三豆腐块阿斯弗", "发哈可是大后方恐惧哈市", "啊开始就好付款计划v卡收费的卡上的机会附加费感觉哈是官方的环境啊是的高房价哈斯夺冠翻噶实践活动覆盖较好啊是固定火箭发射轨道" };
            sresult = of.ToBytes(typeof(string[]), str_inttest, out len);
            string[] str_outtest = null;
            str_outtest = (string[])of.ToObjact(typeof(string[]), sresult);

            TestPacket tp = new TestPacket();
            tp.test1 = 999999;
            byte[] tempb = tp.GetBytes();

            TestPacket tp1 = TestPacket.;
            tp1.CreatePacketFromBytes(tempb);
        }
    }
    public class TestPacket : PacketBase, IPacket
    {
        public int test1 { get; set; } = 0;
    }
}
