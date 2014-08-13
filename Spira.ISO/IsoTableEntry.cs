using System;
using System.Runtime.InteropServices;
using Spira.Core;

namespace Spira.ISO
{
    //[UnsafeCastContainsOnlyValueTypes]
    [StructLayout(LayoutKind.Explicit, Size = 4, Pack = 1)]
    public struct IsoTableEntry
    {
        [FieldOffset(0)] private int _sector;
        [FieldOffset(2)] private byte _flags;
        [FieldOffset(3)] private byte _left;

        public int Sector
        {
            get { return _sector & 0x3FFFFF; }
        }

        public IsoTableEntryFlags Flags
        {
            get { return (IsoTableEntryFlags)(_flags & 0x3); }
        }

        public int Left
        {
            get { return _left * 8; }
        }

        public override string ToString()
        {
            return String.Format("Sector: {0}, Left : {1}, Flags: {2}", Sector, Left, Flags);
        }
    }
}