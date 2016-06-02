using TcpLibrary.Interface;

namespace TcpLibrary.Packet
{
    public class MainPacket<T> : PacketBase, IPacket
    {
        public MainPacket(T commandType, IPacket packet)
        {
            if (commandType.GetHashCode() > -1)
            {
                CommandType = commandType;
                Data = packet.GetBytes();
            }
            else throw new System.Exception("commandType Error");
        }
        public MainPacket() { }
        public T CommandType;
        public byte[] Data = new byte[] { };
    }
}
