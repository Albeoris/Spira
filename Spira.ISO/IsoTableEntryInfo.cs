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
        public IsoTableEntryFlags Flags;
        public bool ImplicitCompressed;
        public FFXFileSignatures Signature;
        public string Sha256Hash;
        public string FilePath;
        public string TruePath;
        public string RelativePath;

        public IsoTableEntryInfo(int index, int defectiveIndex, long offset, long compressedSize, IsoTableEntryFlags flags)
        {
            Index = index;
            DefectiveIndex = defectiveIndex;
            Offset = offset;
            CompressedSize = compressedSize;
            Flags = flags;
        }

        public string GetFileName()
        {
            StringBuilder sb = new StringBuilder(20);
            sb.AppendFormat("F{0:D5}_{1:D5}", Index, DefectiveIndex);
            if (ImplicitCompressed) sb.Append('I');
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
            string hash = null;
            for (int i = 12; i < name.Length && hash == null; i++)
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
                    case '_':
                        hash = name.Substring(i + 1);
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
                ImplicitCompressed = implicitCompressed,
                Signature = signature,
                UncompressedSize = new FileInfo(filePath).Length,
                Sha256Hash = hash,
                FilePath = filePath
            };
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Index);
            bw.Write(DefectiveIndex);
            bw.Write((int)Flags);
            
            bw.Write(UncompressedSize);
            bw.Write(ImplicitCompressed);
            bw.Write((int)Signature);
            bw.Write(Sha256Hash);

            bw.Write(FilePath);
        }
    }
}