﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TcpLibrary.Interface;
using TcpLibrary.Packet;
using TcpLibrary;
namespace Client.Packet
{
    public class TextPacket : PacketBase, IPacket
    {
        public TextPacket() { _Encoding = Encoding.UTF8; }
        public string Text = string.Empty;
        public string[] Strs = new string[0];
    }
}
