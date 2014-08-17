using System.Runtime.InteropServices;

namespace Spira.Battle
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BattleBinFileDescriptorHeader
    {
        public int Unknown00;
        public int Unknown04;
        public int AuthorOffset;
        public int NameOffset;
    }
}