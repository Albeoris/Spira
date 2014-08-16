using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using Spira.Core;

namespace Spira.ISO
{
    public sealed class IsoFileCommander : IProgressSender, IDisposable
    {
        private readonly IsoFileInfo _isoInfo;
        private readonly FileStream _stream;
        private readonly MemoryMappedFile _memory;

        private readonly DisposableStack _disposables = new DisposableStack();

        public event Action<long> ProgressTotalChanged;
        public event Action<long> ProgressIncrement;

        public IsoFileCommander(IsoFileInfo isoInfo)
        {
            _isoInfo = Exceptions.CheckArgumentNull(isoInfo, "isoInfo");

            try
            {
                _stream = _disposables.Add(new FileStream(isoInfo.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                _memory = _disposables.Add(MemoryMappedFile.CreateFromFile(_stream, null, 0, MemoryMappedFileAccess.Read, null, HandleInheritability.Inheritable, false));
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            _disposables.SafeDispose();
        }

        public List<IsoTableEntry> GetEntries()
        {
            ProgressTotalChanged.NullSafeInvoke(_isoInfo.MaxEntriesCount);
            List<IsoTableEntry> entries = new List<IsoTableEntry>(_isoInfo.MaxEntriesCount);

            using (MemoryMappedViewStream input = _memory.CreateViewStream(_isoInfo.EntryTableOffset, _isoInfo.MaxEntriesCount * 4, MemoryMappedFileAccess.Read))
            {
                for (int i = 0; i < _isoInfo.MaxEntriesCount; i++)
                {
                    ProgressIncrement.NullSafeInvoke(1);

                    IsoTableEntry entry = input.ReadStruct<IsoTableEntry>();
                    if (entry.Sector == 0)
                    {
                        ProgressTotalChanged.NullSafeInvoke(i);
                        break;
                    }

                    entries.Add(entry);
                }
            }

            return entries;
        }

        public Stream OpenStream(long offset, long fileSize)
        {
            return _memory.CreateViewStream(offset, fileSize, MemoryMappedFileAccess.Read);
        }

        public List<IsoTableEntryInfo> GetEntriesInfo(IList<IsoTableEntry> entries)
        {
            ProgressTotalChanged.NullSafeInvoke(entries.Count);
            List<IsoTableEntryInfo> result = new List<IsoTableEntryInfo>(entries.Count);

            int counter = 0;
            for (int i = 0; i < entries.Count - 1; i++)
            {
                IsoTableEntry entry = entries[i];
                long compressedSize = (entries[i + 1].Sector - entry.Sector) * _isoInfo.SectorSize - entry.Left;
                if (compressedSize < 1)
                {
                    ProgressIncrement.NullSafeInvoke(1);
                    continue;
                }

                long offset = entry.Sector * _isoInfo.SectorSize;
                IsoTableEntryInfo info = new IsoTableEntryInfo(i, counter++, offset, compressedSize, entry.Flags);

                result.Add(info);
                ProgressIncrement.NullSafeInvoke(1);
            }

            return result;
        }

        public void ReadAdditionalEntriesInfo(IList<IsoTableEntryInfo> infos)
        {
            ProgressTotalChanged.NullSafeInvoke(infos.Count);
            Parallel.ForEach(infos, SafeReadAdditionalEntryInfo);
        }

        private void SafeReadAdditionalEntryInfo(IsoTableEntryInfo info)
        {
            try
            {
                if (info.CompressedSize < 4)
                    return;

                byte[] signature;
                using (Stream input = OpenStream(info.Offset, info.CompressedSize))
                using (MemoryStream output = new MemoryStream(4))
                {
                    if (!info.ImplicitCompressed)
                    {
                        info.ImplicitCompressed = (input.ReadByte() == 0x01);
                        input.Seek(-1, SeekOrigin.Current);
                    }

                    if (info.ImplicitCompressed)
                    {
                        input.Seek(1, SeekOrigin.Current);
                        int uncompressedSize = input.ReadStruct<int>();
                        if (uncompressedSize < 4)
                            return;

                        LZSStream decompressStream = new LZSStream(input, output);
                        decompressStream.Decompress(4);
                        output.Flush();
                        signature = output.ToArray();
                        if (signature.Length == 0)
                            return;
                    }
                    else
                    {
                        signature = new byte[4];
                        input.EnsureRead(signature, 0, 4);
                    }
                }
                info.Signature = (FFXFileSignatures)BitConverter.ToInt32(signature, 0);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read additional info of entry '{0}'", info.GetFileName());
            }
            finally
            {
                ProgressIncrement.NullSafeInvoke(1);
            }
        }

        public void ExtractFile(IsoTableEntryInfo info, string outputPath)
        {
            using (Stream input = OpenStream(info.Offset, info.CompressedSize))
            using (Stream output = File.Create(outputPath))
            {
                if (!info.ImplicitCompressed)
                {
                    input.CopyTo(output);
                }
                else
                {
                    input.Seek(1, SeekOrigin.Current);
                    int uncompressedSize = input.ReadStruct<int>();

                    LZSStream decompressStream = new LZSStream(input, output);
                    decompressStream.Decompress(uncompressedSize);
                }
            }
        }
    }
}