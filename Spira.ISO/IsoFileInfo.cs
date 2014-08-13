using Spira.Core;

namespace Spira.ISO
{
    public sealed class IsoFileInfo
    {
        public readonly string FilePath;
        public readonly string ExecutableFileName = "SLPS_250.88";
        public readonly int EntryTableSector = 280;
        public readonly long SectorSize = 2048;
        public readonly int MaxEntriesCount = 0x4000;

        public IsoFileInfo(string filePath)
        {
            FilePath = Exceptions.CheckFileNotFoundException(filePath);
        }

        public long EntryTableOffset
        {
            get { return EntryTableSector * SectorSize; }
        }
    }
}