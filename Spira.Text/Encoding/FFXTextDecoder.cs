using Spira.Core;

namespace Spira.Text
{
    public sealed class FFXTextDecoder
    {
        private readonly FFXTextEncodingCodepage _codepage;

        public FFXTextDecoder(FFXTextEncodingCodepage codepage)
        {
            _codepage = Exceptions.CheckArgumentNull(codepage, "codepage");
        }

        public int GetMaxCharCount(int byteCount)
        {
            return byteCount * FFXTextTag.MaxTagLength;
        }

        public int GetCharCount(byte[] bytes, int index, int count)
        {
            int result = 0;

            char[] buff = new char[FFXTextTag.MaxTagLength];
            while (count > 0)
            {
                FFXTextTag tag = FFXTextTag.TryRead(bytes, ref index, ref count);
                if (tag != null)
                {
                    int offset = 0;
                    result += tag.Write(buff, ref offset);
                }
                else
                {
                    count--;
                    result++;
                    index++;
                }
            }

            return result;
        }

        public int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int result = 0;

            while (byteCount > 0)
            {
                FFXTextTag tag = FFXTextTag.TryRead(bytes, ref byteIndex, ref byteCount);
                if (tag != null)
                {
                    result += tag.Write(chars, ref charIndex);
                }
                else
                {
                    chars[charIndex++] = _codepage[bytes[byteIndex++]];
                    byteCount--;
                    result++;
                }
            }

            return result;
        }
    }
}