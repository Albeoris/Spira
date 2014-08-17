using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Spira.Core;

namespace Spira.ISO
{
    public sealed class IsoInfo
    {
        public readonly string FilePath;
        public readonly string ExecutableFileName = "SLPS_250.88";
        public readonly int EntryTableSector = 280;
        public readonly long SectorSize = 2048;
        public readonly int MaxEntriesCount = 0x4000;

        public IsoInfo(string filePath)
        {
            FilePath = Exceptions.CheckFileNotFoundException(filePath);
        }

        public long EntryTableOffset
        {
            get { return EntryTableSector * SectorSize; }
        }

        public Dictionary<int, string> GetKnownFilePathes()
        {
            Dictionary<int, string> result = new Dictionary<int, string>(1500);
            
            using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream("Spira.ISO." + ExecutableFileName.Replace(".", "._") + ".FileNames.bin"))
            {
                if (input == null)
                    return result;

                using (BinaryReader br = new BinaryReader(input))
                {
                    while (!input.IsEndOfStream())
                    {
                        int index = br.ReadInt32();
                        int defectiveIndex = br.ReadInt32();
                        string path = br.ReadString();
                        result.Add(index, path);
                    }
                }
            }
            
            return result;
        }
    }
}