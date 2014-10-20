using System.Runtime.InteropServices;

namespace Spira.Images
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EventFileHeader
    {
        public int Signature;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0F)] public int[] Offsets;

        public int DescriptorOffset
        {
            get { return Offsets[0]; }
        }

        public int FtcxOffset
        {
            get { return Offsets[3]; }
        }

        public int TextOffset
        {
            get { return Offsets[4]; }
        }
    }
}