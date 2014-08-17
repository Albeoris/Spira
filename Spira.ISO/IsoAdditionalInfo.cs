using System;

namespace Spira.ISO
{
    [Flags]
    public enum IsoAdditionalInfo
    {
        None = 0,
        PS2KnownSignature = 1,
        PS2KnownName = 2,
        PS3KnownPath = 4,
        PS3Identical = 8,
    }
}