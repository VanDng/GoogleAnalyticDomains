using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiresharkFeeder;

namespace WiresharkFeeder
{
    delegate void NewPacketArrived(byte[] buffer, int offset, int count);

    interface IPCAPPacketSource
    {
        public event NewPacketArrived OnNewPacketArrived;
        public bool GetGlobalHeader(ref byte[] buffer, ref int offset, ref int count);
        public bool GetNextCapturePacket(ref byte[] buffer, ref int offset, ref int count);
        public void ResendGlobalHeaderPacket();
    }
}
