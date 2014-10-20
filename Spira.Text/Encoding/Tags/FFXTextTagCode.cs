namespace Spira.Text
{
    public enum FFXTextTagCode
    {
        // Без параметров
        End = 0x00,
        Next = 0x01,
        Line = 0x03,
        Point = 0x8E,
        Music = 0x91,
        Heart = 0x92,
        Exclamation = 0x98,
        Up = 0x99,
        Down = 0x9A,
        Left = 0x9B,
        Right = 0x9C,
        Beta = 0xB9,
        Func = 0xD1,
        Corp = 0xDD,
        Reg = 0xDF,

        // С параметром (байт)
        Color = 0x0A,
        Var07 = 0x07,
        Var12 = 0x12,

        // С параметром (байт - 0x30)
        Choise = 0x10,

        // С параметром (именованый)
        System = 0x09,
        Font = 0x0E,
        Area = 0x20,
        Item = 0x23,
        Char = 0x13,
        Npc = 0x19,
        Key = 0x0B
    }
}