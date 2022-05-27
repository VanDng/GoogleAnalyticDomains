using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiresharkFeeder
{
    class PCAPGlobalHeaderPacket
    {
        public uint magic_number;           /* magic number */
        public ushort version_major;        /* major version number */
        public ushort version_minor;        /* minor version number */
        public uint thiszone;               /* GMT to local correction */
        public uint sigfigs;                /* accuracy of timestamps */
        public uint capturePacketLength;    /* max length of captured packets, in octets/bytes */
        public uint datalinkType;           /* data link type */

        public PCAPGlobalHeaderPacket()
        {
            magic_number = 0xa1b2c3d4;
            version_major = 2;
            version_minor = 4;
            thiszone = 0;
            sigfigs = 0;
            capturePacketLength = 0;
            datalinkType = 0;
        }

        public void FromBytes(byte[] bytes)
        {
            using Stream stream = new MemoryStream(bytes, false);
            using BinaryReader reader = new BinaryReader(stream);

            magic_number = reader.ReadUInt32();
            version_major = reader.ReadUInt16();
            version_minor = reader.ReadUInt16();
            thiszone = reader.ReadUInt32();
            sigfigs = reader.ReadUInt32();
            capturePacketLength = reader.ReadUInt32();
            datalinkType = reader.ReadUInt32();
        }
    }

    class PCAPCaptureHeaderPacket
    {
        public uint ts_sec;         /* timestamp seconds */
        public uint ts_usec;        /* timestamp microseconds */
        public uint incl_len;       /* number of octets of packet saved in file */
        public uint orig_len;       /* actual length of packet */

        public void FromBytes(byte[] bytes)
        {
            using Stream stream = new MemoryStream(bytes, false);
            using BinaryReader reader = new BinaryReader(stream);

            ts_sec = reader.ReadUInt32();
            ts_usec = reader.ReadUInt32();
            incl_len = reader.ReadUInt32();
            orig_len = reader.ReadUInt32();
        }
    }
}
