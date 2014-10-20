using System.Runtime.InteropServices;

namespace Spira.Images
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EventFileDescriptorHeader
    {
        public int Unknown00;
        public int Unknown04;
        public int AuthorOffset;
        public int NameOffset;
    }
}