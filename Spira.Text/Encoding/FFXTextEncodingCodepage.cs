using System;
using System.Collections.Generic;
using System.Xml;
using Spira.Core;

namespace Spira.Text
{
    public sealed class FFXTextEncodingCodepage
    {
        private readonly char[] _chars;
        private readonly Dictionary<char, byte> _bytes;

        public FFXTextEncodingCodepage(char[] chars, Dictionary<char, byte> bytes)
        {
            _chars = Exceptions.CheckArgumentNull(chars, "chars");
            _bytes = Exceptions.CheckArgumentNull(bytes, "bytes");
        }

        public char this[byte b]
        {
            get
            {
                char c = _chars[b];
                if (c == '\0')
                    c = 'Ё';
                    //throw new ArgumentOutOfRangeException("b", b, "Символ соответствующий заданному байту не задан.");
                return c;
            }
        }

        public byte this[char c]
        {
            get { return _bytes[c]; }
        }

        public char? TryGetChar(byte b)
        {
            char c = _chars[b];
            if (c == '\0')
                return null;
            return c;
        }

        public byte? TryGetByte(char c)
        {
            byte b;
            if (_bytes.TryGetValue(c, out b))
                return b;
            return null;
        }

        public void GetParameters(out char[] chars, out HashSet<char>[] bytes)
        {
            chars = (char[])_chars.Clone();

            bytes = new HashSet<char>[256];
            for (int i = 0; i < 256; i++)
                bytes[i] = new HashSet<char>();

            foreach (KeyValuePair<char, byte> pair in _bytes)
                bytes[pair.Value].Add(pair.Key);
        }

        public static FFXTextEncodingCodepage Create()
        {
            char[] chars =
            {
                '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ', '!', '”', '#', '$', '%',
                '&', '’', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
                'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[', '\\', ']', '^', '_', '‘',
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
                'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '{', '|', '}', '~', '·', '【',
                '】', '♪', '♥', '☻', '“', '”', '—', '☻', '¡', '↑', '↓', '←', '→', '¨', '«', 'º',
                '☻', '»', '¿', 'À', 'Á', 'Â', 'Ä', 'ç', 'È', 'É', 'Ê', 'Ë', 'Ì', 'Í', 'Î', 'Ï',
                'Ñ', 'Ò', 'Ó', 'Ô', 'Ö', 'Ù', 'Ú', 'Û', 'Ü', 'ß', 'à', 'á', 'â', 'ä', 'ç', 'è',
                'é', 'ê', 'ë', 'ì', 'í', 'î', 'ï', 'ñ', 'ò', 'ó', 'ô', 'ö', 'ù', 'ú', 'û', 'ü',
                ',', 'ƒ', '„', '…', '\'', '’', '•', '-', '~', '™', '☻', '›', '§', '©', 'ª', '®',
                '±', '²', '³', '¼', '½', '¾', '×', '÷', '‹', '…', '☻', 'ǎ', '★', '☆', '■', '∞',
                '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
            };

            Dictionary<char, byte> bytes = new Dictionary<char, byte>(chars.Length);
            for (int i = chars.Length - 1; i >= 0; i--)
            {
                char ch = chars[i];
                switch (ch)
                {
                    case '¥':
                    case '☻':
                    case 'ⱷ':
                        chars[i] = '\0';
                        continue;
                    case '\0':
                        continue;
                }
                bytes[chars[i]] = (byte)i;
            }

            bytes['А'] = bytes['A'];
            bytes['В'] = bytes['B'];
            bytes['Е'] = bytes['E'];
            bytes['К'] = bytes['K'];
            bytes['М'] = bytes['M'];
            bytes['Н'] = bytes['H'];
            bytes['О'] = bytes['O'];
            bytes['Р'] = bytes['P'];
            bytes['С'] = bytes['C'];
            bytes['Т'] = bytes['T'];
            bytes['Х'] = bytes['X'];
            bytes['а'] = bytes['a'];
            bytes['е'] = bytes['e'];
            bytes['и'] = bytes['u'];
            bytes['о'] = bytes['o'];
            bytes['р'] = bytes['p'];
            bytes['с'] = bytes['c'];
            bytes['у'] = bytes['y'];
            bytes['х'] = bytes['x'];

            return new FFXTextEncodingCodepage(chars, bytes);
        }

        public static FFXTextEncodingCodepage Unserialize(XmlElement options)
        {
            XmlElement encoding = options["Codepage"];
            if (encoding == null)
                return null;

            XmlElement charsNode = encoding.GetChildElement("Chars");
            XmlElement bytesNode = encoding.GetChildElement("Bytes");
            if (charsNode.ChildNodes.Count != 256) throw Exceptions.CreateException("Неверное число дочерних элементов узла '{0}': {1}. Ожидается: 256", charsNode.Name, charsNode.ChildNodes.Count);

            char[] chars = new char[256];
            Dictionary<char, byte> bytes = new Dictionary<char, byte>(256);

            for (int i = 0; i < chars.Length; i++)
            {
                XmlElement charNode = (XmlElement)charsNode.ChildNodes[i];
                chars[i] = charNode.GetChar("Char");
            }

            foreach (XmlElement byteNode in bytesNode)
                bytes[byteNode.GetChar("Char")] = byteNode.GetByte("Byte");

            return new FFXTextEncodingCodepage(chars, bytes);
        }

        public void Serial(XmlElement parent)
        {
            XmlElement encoding = parent.EnsureChildElement("Codepage");
            encoding.RemoveAll();

            XmlElement charsNode = encoding.EnsureChildElement("Chars");
            XmlElement bytesNode = encoding.EnsureChildElement("Bytes");

            foreach (char ch in _chars)
            {
                XmlElement charNode = charsNode.CreateChildElement("Entry");
                charNode.SetChar("Char", ch);
            }

            foreach (KeyValuePair<char, byte> pair in _bytes)
            {
                XmlElement byteNode = bytesNode.CreateChildElement("Entry");
                byteNode.SetChar("Char", pair.Key);
                byteNode.SetByte("Byte", pair.Value);
            }
        }
    }
}