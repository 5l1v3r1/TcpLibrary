using System;
using System.Collections.Generic;
using System.Text;

namespace TcpLibrary.Interface
{
    public interface IConvert
    {
        byte[] Encode(byte[] src);
        byte[] Decode(byte[] src);
    }
}
