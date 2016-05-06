using TcpLibrary.Converter;
using TcpLibrary.Interface;

namespace TcpLibrary
{
    public static class TcpConfig
    {
        public static IConvert Convert = new DefaultConvert();
        public static int ServerMaxClient = 1024;
    }
}
