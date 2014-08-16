using System.Runtime.InteropServices;

namespace Spira.Images
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class FtcxFileHeader
    {
        public int Signature; // 0x58435446
        public short Unknown1; // 0x00C8
        public short Unknwon2; // 0x0614
        public int Unknown3; // 0x00000002
        public int Unknown4; // 0x00000000
        public int Unknown5; // 0x00000043
        public short Unknown6; // 0x000E
        public short Unknown7; // 0x0012
        public int Unknown8; // 0x00000000
        public int Unknown9; // 0x00000000
        public int BlockSize; // 0x00000040
        public short Unknown10; // 0x1200
        public short Unknown11; // 0x0000
        public short Unknown12; // 0x0080
        public short BlockCount; // 0x0012, 0x0048
        public int Unknown13; // 0x00000000
        public int Unknown14; // 0x00001240
        public int UnknownSize; // 0x00000050
        public int Unknown15; // 0x00000050
        public int Unknown16; // 0x00000050
    }
}