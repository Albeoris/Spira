﻿using System;

namespace Spira.ISO
{
    [Flags]
    public enum IsoTableEntryFlags
    {
        // Не соответствует действительности
        None = 0,
        Compressed = 0x1,
        Dummy = 0x2
    }
}