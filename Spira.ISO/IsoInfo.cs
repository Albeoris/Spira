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

        public void FillKnownFileInformation(List<IsoTableEntryInfo> infos)
        {
            Dictionary<int, IsoTableEntryInfo> dic = new Dictionary<int, IsoTableEntryInfo>(infos.Count);
            foreach (IsoTableEntryInfo info in infos)
                dic.Add(info.Index, info);
            
            using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream("Spira.ISO." + ExecutableFileName.Replace(".", "._") + ".AdditionalFileInformation.bin"))
            {
                if (input == null)
                    return;

                using (BinaryReader br = new BinaryReader(input))
                {
                    while (!input.IsEndOfStream())
                    {
                        int index = br.ReadInt32();
                        int defective = br.ReadInt32();
                        IsoTableEntryInfo info = dic[index];
                        
                        info.AdditionalInfo = (IsoAdditionalInfo)br.ReadInt32();
                        if ((info.AdditionalInfo & IsoAdditionalInfo.PS2KnownSignature) == IsoAdditionalInfo.PS2KnownSignature)
                            info.Signature = (FFXFileSignatures)br.ReadInt32();
                        if ((info.AdditionalInfo & IsoAdditionalInfo.PS2KnownName) == IsoAdditionalInfo.PS2KnownName)
                            info.PS2KnownName = br.ReadString();
                        if ((info.AdditionalInfo & IsoAdditionalInfo.PS3KnownPath) == IsoAdditionalInfo.PS3KnownPath)
                            info.PS3KnownPath = br.ReadString();
                    }
                }
            }
        }
    }
}