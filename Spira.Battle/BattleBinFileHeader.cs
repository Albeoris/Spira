using System.Runtime.InteropServices;

namespace Spira.Battle
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BattleBinFileHeader
    {
        public int Count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public int[] Offsets;

        public int DescriptorOffset
        {
            get { return Offsets[0]; }
        }

        public int FtcxOffset
        {
            get { return Offsets[5]; }
        }

        public int TextOffset
        {
            get { return Offsets[6]; }
        }
    }
}
