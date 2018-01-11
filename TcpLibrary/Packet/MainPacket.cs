using TcpLibrary.Interface;

namespace TcpLibrary.Packet
{
    public class MainPacket<T> : PacketBase, IPacket where T : struct
    {
        public MainPacket(T commandType, IPacket packet)
        {
            CommandType = commandType;
            Data = packet.GetBytes();
        }
        public MainPacket() { }
        public T CommandType;
        public byte[] Data = new byte[] { };
    }
}
