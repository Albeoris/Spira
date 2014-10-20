using System;
using System.Globalization;
using System.Text;
using Spira.Core;

namespace Spira.Text
{
    public sealed class FFXTextTag
    {
        public static string[] PageSeparator = {new FFXTextTag(FFXTextTagCode.Next).ToString()};
        public static string[] LineSeparator = {new FFXTextTag(FFXTextTagCode.Line).ToString()};

        public const int MaxTagLength = 32;

        public readonly FFXTextTagCode Code;
        public readonly Enum Param;

        public FFXTextTag(FFXTextTagCode code, Enum param = null)
        {
            Code = code;
            Param = param;
        }

        public int Write(byte[] bytes, ref int offset)
        {
            bytes[offset++] = (byte)Code;
            if (Param == null)
                return 1;

            bytes[offset++] = (byte)(FFXTextTagParam)Param;
            return 2;
        }

        public int Write(char[] chars, ref int offset)
        {
            StringBuilder sb = new StringBuilder(MaxTagLength);
            sb.Append('{');
            sb.Append(Code);
            if (Param != null)
            {
                sb.Append(' ');
                sb.Append(Param);
            }
            sb.Append('}');

            if (sb.Length > MaxTagLength)
                throw Exceptions.CreateException("Слишком длинное имя тэга: {0}", sb.ToString());

            for (int i = 0; i < sb.Length; i++)
                chars[offset++] = sb[i];

            return sb.Length;
        }

        public static FFXTextTag TryRead(byte[] bytes, ref int offset, ref int left)
        {
            FFXTextTagCode code = (FFXTextTagCode)bytes[offset++];
            left -= 2;
            switch (code)
            {
                case FFXTextTagCode.End:
                case FFXTextTagCode.Next:
                case FFXTextTagCode.Line:
                case FFXTextTagCode.Point:
                case FFXTextTagCode.Music:
                case FFXTextTagCode.Heart:
                case FFXTextTagCode.Exclamation:
                case FFXTextTagCode.Up:
                case FFXTextTagCode.Down:
                case FFXTextTagCode.Left:
                case FFXTextTagCode.Right:
                case FFXTextTagCode.Beta:
                case FFXTextTagCode.Func:
                case FFXTextTagCode.Corp:
                case FFXTextTagCode.Reg:
                    left++;
                    return new FFXTextTag(code);
                case FFXTextTagCode.Var07:
                case FFXTextTagCode.Var12:
                case FFXTextTagCode.Color:
                    return new FFXTextTag(code, (FFXTextTagParam)bytes[offset++]);
                case FFXTextTagCode.Choise:
                    return new FFXTextTag(code, (FFXTextTagParam)bytes[offset++] - 0x30);
                case FFXTextTagCode.Char:
                    return new FFXTextTag(code, (FFXTextTagCharacter)bytes[offset++]);
                case FFXTextTagCode.Npc:
                    return new FFXTextTag(code, (FFXTextTagNpc)bytes[offset++]);
                case FFXTextTagCode.Key:
                    return new FFXTextTag(code, (FFXTextTagKey)bytes[offset++]);
                case FFXTextTagCode.System:
                    return new FFXTextTag(code, (FFXTextTagSystem)bytes[offset++]);
                case FFXTextTagCode.Font:
                    return new FFXTextTag(code, (FFXTextTagFont)bytes[offset++]);
                case FFXTextTagCode.Area:
                    return new FFXTextTag(code, (FFXTextTagArea)bytes[offset++]);
                case FFXTextTagCode.Item:
                    return new FFXTextTag(code, (FFXTextTagItem)bytes[offset++]);
                default:
                    left += 2;
                    offset--;
                    return null;
            }
        }

        public static FFXTextTag TryRead(char[] chars, ref int offset, ref int left)
        {
            int oldOffset = offset;
            int oldleft = left;

            string tag, par;
            if (chars[offset++] != '{' || !TryGetTag(chars, ref offset, ref left, out tag, out par))
            {
                offset = oldOffset;
                left = oldleft;
                return null;
            }

            FFXTextTagCode? code = EnumCache<FFXTextTagCode>.TryParse(tag);
            if (code == null)
            {
                offset = oldOffset;
                left = oldleft;
                return null;
            }

            switch (code.Value)
            {
                case FFXTextTagCode.End:
                case FFXTextTagCode.Next:
                case FFXTextTagCode.Line:
                case FFXTextTagCode.Point:
                case FFXTextTagCode.Music:
                case FFXTextTagCode.Heart:
                case FFXTextTagCode.Exclamation:
                case FFXTextTagCode.Up:
                case FFXTextTagCode.Down:
                case FFXTextTagCode.Left:
                case FFXTextTagCode.Right:
                case FFXTextTagCode.Beta:
                case FFXTextTagCode.Func:
                case FFXTextTagCode.Corp:
                case FFXTextTagCode.Reg:
                    return new FFXTextTag(code.Value);
                case FFXTextTagCode.Var07:
                case FFXTextTagCode.Var12:
                case FFXTextTagCode.Color:
                {
                    byte numArg;
                    if (byte.TryParse(par, NumberStyles.Integer, CultureInfo.InvariantCulture, out numArg))
                        return new FFXTextTag(code.Value, (FFXTextTagParam)numArg);
                    break;
                }
                case FFXTextTagCode.Choise:
                {
                    byte numArg;
                    if (byte.TryParse(par, NumberStyles.Integer, CultureInfo.InvariantCulture, out numArg))
                        return new FFXTextTag(code.Value, (FFXTextTagParam)numArg + 0x30);
                    break;
                }
                case FFXTextTagCode.System:
                {
                    FFXTextTagSystem? arg = EnumCache<FFXTextTagSystem>.TryParse(par);
                    if (arg != null) return new FFXTextTag(code.Value, arg.Value);
                    break;
                }
                case FFXTextTagCode.Font:
                {
                    FFXTextTagFont? arg = EnumCache<FFXTextTagFont>.TryParse(par);
                    if (arg != null) return new FFXTextTag(code.Value, arg);
                    break;
                }
                case FFXTextTagCode.Area:
                {
                    FFXTextTagArea? arg = EnumCache<FFXTextTagArea>.TryParse(par);
                    if (arg != null) return new FFXTextTag(code.Value, arg);
                    break;
                }
                case FFXTextTagCode.Item:
                {
                    FFXTextTagItem? arg = EnumCache<FFXTextTagItem>.TryParse(par);
                    if (arg != null) return new FFXTextTag(code.Value, arg);
                    break;
                }
                case FFXTextTagCode.Char:
                {
                    FFXTextTagCharacter? arg = EnumCache<FFXTextTagCharacter>.TryParse(par);
                    if (arg != null) return new FFXTextTag(code.Value, arg.Value);
                    break;
                }
                case FFXTextTagCode.Npc:
                {
                    FFXTextTagNpc? arg = EnumCache<FFXTextTagNpc>.TryParse(par);
                    if (arg != null) return new FFXTextTag(code.Value, arg.Value);
                    break;
                }
                case FFXTextTagCode.Key:
                {
                    FFXTextTagKey? arg = EnumCache<FFXTextTagKey>.TryParse(par);
                    if (arg != null) return new FFXTextTag(code.Value, arg.Value);
                    break;
                }


            }

            offset = oldOffset;
            left = oldleft;
            return null;
        }

        private static bool TryGetTag(char[] chars, ref int offset, ref int left, out string tag, out string par)
        {
            int lastIndex = Array.IndexOf(chars, '}', offset);
            int length = lastIndex - offset + 1;
            if (length < 2)
            {
                tag = null;
                par = null;
                return false;
            }

            left--;
            left -= length;

            int spaceIndex = Array.IndexOf(chars, ' ', offset + 1, length - 2);
            if (spaceIndex < 0)
            {
                tag = new string(chars, offset, length - 1);
                par = string.Empty;
            }
            else
            {
                tag = new string(chars, offset, spaceIndex - offset);
                par = new string(chars, spaceIndex + 1, lastIndex - spaceIndex - 1);
            }

            offset = lastIndex + 1;
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(MaxTagLength);
            sb.Append('{');
            sb.Append(Code);
            if (Param != null)
            {
                sb.Append(' ');
                sb.Append(Param);
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}