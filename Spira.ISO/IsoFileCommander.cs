using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using Spira.Core;

namespace Spira.ISO
{
    public sealed class IsoFileCommander : IDisposable
    {
        private readonly IsoFileInfo _isoInfo;
        private readonly FileStream _stream;
        private readonly MemoryMappedFile _memory;

        private readonly DisposableStack _disposables = new DisposableStack();

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
            List<IsoTableEntry> entries = new List<IsoTableEntry>(_isoInfo.MaxEntriesCount);

            using (MemoryMappedViewStream input = _memory.CreateViewStream(_isoInfo.EntryTableOffset, _isoInfo.MaxEntriesCount * 4, MemoryMappedFileAccess.Read))
            {
                for (int i = 0; i < _isoInfo.MaxEntriesCount; i++)
                {
                    IsoTableEntry entry = input.ReadStruct<IsoTableEntry>();
                    if (entry.Sector == 0)
                        break;

                    entries.Add(entry);
                }
            }

            return entries;
        }

        public Stream OpenStream(long offset, long fileSize)
        {
            return _memory.CreateViewStream(offset, fileSize, MemoryMappedFileAccess.Read);
        }
    }
}