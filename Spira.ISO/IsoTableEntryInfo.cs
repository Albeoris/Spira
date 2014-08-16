using System.Globalization;
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
        public IsoTableEntryFlags Flags;
        public bool ImplicitCompressed;
        public FFXFileSignatures Signature;

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
    }
}