using System;
using System.IO;
using System.Text;
using Spira.Core;

namespace Spira.Battle
{
    public sealed class BattleBinFileReader
    {
        private readonly Stream _input;
        private BattleBinFileHeader _header;
        private BattleBinFileDescriptorHeader _descriptorHeader;

        public BattleBinFileReader(Stream input)
        {
            _input = Exceptions.CheckArgumentNull(input, "input");
        }

        public BattleBinFileHeader ReadHeader()
        {
            _header = _input.ReadStruct<BattleBinFileHeader>();
            if (_header.Count != 8) throw new Exception();
            if (_header.DescriptorOffset != 0x30) throw new NotSupportedException();
            return _header;
        }

        public BattleBinFileDescriptorHeader ReadDescriptorHeader()
        {
            return _descriptorHeader = _input.ReadStruct<BattleBinFileDescriptorHeader>();
        }

        public string ReadFileName()
        {
            _input.Seek(_descriptorHeader.NameOffset + _header.DescriptorOffset, SeekOrigin.Begin);
            return _input.ReadNullTerminatedString(Encoding.UTF8);
        }
    }
}