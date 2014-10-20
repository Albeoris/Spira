using System;
using System.IO;
using System.Text;
using Spira.Core;

namespace Spira.Images
{
    public sealed class EventFileReader
    {
        private readonly Stream _input;
        private EventFileHeader _header;
        private EventFileDescriptorHeader _descriptorHeader;

        public EventFileReader(Stream input)
        {
            _input = Exceptions.CheckArgumentNull(input, "input");
        }

        public EventFileHeader ReadHeader()
        {
            _header = _input.ReadStruct<EventFileHeader>();
            if (_header.Signature != (int)FFXFileSignatures.Ebp) throw new Exception();
            if (_header.DescriptorOffset != 0x40) throw new NotSupportedException();
            return _header;
        }

        public EventFileDescriptorHeader ReadDescriptorHeader()
        {
            return _descriptorHeader = _input.ReadStruct<EventFileDescriptorHeader>();
        }

        public string ReadFileName()
        {
            _input.Seek(_descriptorHeader.NameOffset + _header.DescriptorOffset, SeekOrigin.Begin);
            return _input.ReadNullTerminatedString(Encoding.UTF8);
        }
    }
}