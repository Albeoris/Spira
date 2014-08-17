using System;
using System.IO;
using System.Text;
using Spira.Core;

namespace Spira.ISO
{
    public sealed class IsoTableEntryInfo
    {
        public int Index;
        public int DefectiveIndex;
        public long Offset;
        public long CompressedSize;
        public long UncompressedSize;
        public bool IsCompressed;
        public IsoTableEntryFlags Flags;
        public FFXFileSignatures Signature;
        public IsoAdditionalInfo AdditionalInfo;
        public string PS2KnownName;
        public string PS3KnownPath;

        public IsoTableEntryInfo(int index, int defectiveIndex, long offset, long compressedSize, IsoTableEntryFlags flags)
        {
            Index = index;
            DefectiveIndex = defectiveIndex;
            Offset = offset;
            CompressedSize = compressedSize;
            Flags = flags;
        }

        public string GetRelativePath()
        {
            if (!string.IsNullOrEmpty(PS3KnownPath))
                return PS3KnownPath;

            string folder = EnumCache<FFXFileSignatures>.IsDefined(Signature) ? Signature.ToString().ToUpper() : "Unknown";
            return Path.Combine(folder, string.IsNullOrEmpty(PS2KnownName) ? GetFileName() : PS2KnownName + '.' + folder.ToLower());
        }

        public string GetFileName()
        {
            StringBuilder sb = new StringBuilder(20);
            sb.AppendFormat("F{0:D5}_{1:D5}", Index, DefectiveIndex);
            if (IsCompressed) sb.Append('I');
            if ((Flags & IsoTableEntryFlags.Compressed) == IsoTableEntryFlags.Compressed) sb.Append('C');
            if ((Flags & IsoTableEntryFlags.Dummy) == IsoTableEntryFlags.Dummy) sb.Append('D');
            if (EnumCache<FFXFileSignatures>.IsDefined(Signature)) sb.Append('.').Append(Signature.ToString().ToLowerInvariant());
            return sb.ToString();
        }

        public static IsoTableEntryInfo TryParse(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            if (name[0] != 'F' || name[6] != '_') return null;

            int index = int.Parse(name.Substring(1, 5));
            int defectiveIndex = int.Parse(name.Substring(7, 5));

            bool implicitCompressed = false;
            IsoTableEntryFlags flags = IsoTableEntryFlags.None;
            for (int i = 12; i < name.Length; i++)
            {
                switch (name[i])
                {
                    case 'I':
                        implicitCompressed = true;
                        break;
                    case 'C':
                        flags |= IsoTableEntryFlags.Compressed;
                        break;
                    case 'D':
                        flags |= IsoTableEntryFlags.Dummy;
                        break;
                }
            }

            FFXFileSignatures signature = 0;
            string ext = Path.GetExtension(filePath);
            if (!string.IsNullOrEmpty(ext))
            {
                FFXFileSignatures? tag = EnumCache<FFXFileSignatures>.TryParse(ext.Substring(1));
                if (tag == null)
                    return null;
                signature = tag.Value;
            }

            return new IsoTableEntryInfo(index, defectiveIndex, -1, -1, flags)
            {
                IsCompressed = implicitCompressed,
                Signature = signature,
                UncompressedSize = new FileInfo(filePath).Length,
            };
        }
    }
}