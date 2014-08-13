using System;
using System.IO;
using Spira.Core;

namespace Spira.ISO
{
    public sealed class LZSStream
    {
        private readonly Stream _input;
        private readonly Stream _output;
        private readonly CircularBuffer<byte> _circularBuffer;

        public event EventHandler<int> ReverseProgress;

        public LZSStream(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (output == null)
                throw new ArgumentNullException("output");

            if (!input.CanRead)
                throw new ArgumentException("Входной поток не поддерживает чтения.", "input");
            if (!output.CanWrite)
                throw new ArgumentException("Выходной поток не поддерживает записи.", "input");

            _input = input;
            _output = output;
            _circularBuffer = new CircularBuffer<byte>(4096);
        }

        public void Decompress(int unpackedLength)
        {
            while (unpackedLength > 0)
            {
                int flag = (byte)_input.ReadByte();
                if (flag == 0)
                    return;

                // RAW
                if (flag < 0x7E)
                {
                    while (flag-- > 0)
                    {
                        byte b = (byte)_input.ReadByte();
                        _circularBuffer.Write(b);
                        _output.WriteByte(b);
                        unpackedLength--;
                    }
                    continue;
                }
                // RLE
                if (flag >= 0x7E && flag <= 0x7F)
                {
                    int length = _input.ReadByte();
                    if (flag == 0x7E) // Однобайтная длина
                        length += 4;
                    else
                        length |= (_input.ReadByte() << 8); // Двухбайтная длина
                    
                    byte b = (byte)_input.ReadByte();
                    while (length-- > 0)
                    {
                        _circularBuffer.Write(b);
                        _output.WriteByte(b);
                        unpackedLength--;
                    }
                    continue;
                }

                // LZ
                flag -= 0x80;
                int offset = (byte)_input.ReadByte() + (flag % 0x8) * 0x100;
                flag = (byte)(3 + (flag / 0x8));

                for (int i = 0; i < flag; i++)
                {
                    int index = (int)(_circularBuffer.Index - offset - 1);
                    byte b = _circularBuffer.GetByOffset(index);
                    _circularBuffer.Write(b);
                    _output.WriteByte(b);
                    unpackedLength--;
                }
            }

            ReverseProgress.NullSafeInvoke(this, unpackedLength);
        }
    }
}