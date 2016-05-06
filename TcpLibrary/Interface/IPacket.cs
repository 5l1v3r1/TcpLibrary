using System;
using System.Collections.Generic;
using System.Text;

namespace TcpLibrary.Interface
{
    public interface IPacket
    {
        byte[] GetBytes();
    }
}
