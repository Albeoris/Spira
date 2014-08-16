using System.IO;
using Spira.Core;

namespace Spira.Images
{
    public sealed class FtcxFileReader
    {
        private readonly Stream _input;
        private FtcxFileHeader _header;

        public FtcxFileReader(Stream input)
        {
            _input = Exceptions.CheckArgumentNull(input, "input");
        }

        public FtcxFileHeader ReadHeader()
        {
            return _header = _input.ReadStruct<FtcxFileHeader>();
        }

        public byte[] ReadUnknownSubHeader()
        {
            byte[] buff = new byte[_header.UnknownSize];
            _input.EnsureRead(buff, 0, buff.Length);
            return buff;
        }

        public void SkipUnknownSubHeader()
        {
            _input.Seek(_header.UnknownSize, SeekOrigin.Current);
        }

        public int GetImageSize()
        {
            return _header.BlockCount * _header.BlockSize * 2;
        }

        public unsafe SafeHGlobalHandle ReadImage()
        {
            SafeHGlobalHandle result = new SafeHGlobalHandle(GetImageSize());
            byte* ptr = (byte*)result.DangerousGetHandle().ToPointer();

            using (DisposableAction insurance = new DisposableAction(result.Dispose))
            {
                for (int y = 0; y < _header.BlockCount; y++)
                {
                    byte* blockPtr = ptr + y * _header.BlockSize * 2;

                    for (int x = 0, i = 0; x < _header.BlockSize; x++)
                    {
                        byte b = (byte)_input.ReadByte();

                        *(blockPtr + i) = (byte)(b & 0x33);
                        *(blockPtr + i + 8) = (byte)((b >> 2) & 0x33);
                        if (++i % 8 == 0)
                            i += 8;
                    }
                }

                insurance.Cancel();
            }

            return result;
        }
    }
}