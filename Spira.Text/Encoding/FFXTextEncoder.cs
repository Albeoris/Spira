using Spira.Core;

namespace Spira.Text
{
    public sealed class FFXTextEncoder
    {
        private readonly FFXTextEncodingCodepage _codepage;

        public FFXTextEncoder(FFXTextEncodingCodepage codepage)
        {
            _codepage = Exceptions.CheckArgumentNull(codepage, "codepage");
        }

        public int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        public int GetByteCount(char[] chars, int index, int count)
        {
            int result = 0;

            byte[] buff = new byte[2];
            while (count > 0)
            {
                FFXTextTag tag = FFXTextTag.TryRead(chars, ref index, ref count);
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

        public int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int result = 0;

            while (charCount > 0)
            {
                FFXTextTag tag = FFXTextTag.TryRead(chars, ref charIndex, ref charCount);
                if (tag != null)
                {
                    result += tag.Write(bytes, ref byteIndex);
                }
                else
                {
                    bytes[byteIndex++] = _codepage[chars[charIndex++]];
                    charCount--;
                    result++;
                }
            }

            return result;
        }
    }
}