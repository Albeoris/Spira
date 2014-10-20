using System.ComponentModel.DataAnnotations;

namespace Spira.Text
{
    //0x0B
    public enum FFXTextTagKey : byte
    {
        Triangle = 0x30,
        Circle = 0x31,
        Cross = 0x32,
        Square = 0x33,
        L1 = 0x34,
        R1 = 0x35,
        L2 = 0x36,
        R2 = 0x37,
        Start = 0x38,
        Select = 0x39,
        Direction = 0x40,
        Up = 0x41,
        Right = 0x42,
        UpRight = 0x43,
        Down = 0x44,
        UpDown = 0x45,
        DownRight = 0x46,
        UpRightDown = 0x47,
        Left = 0x48,
        UpLeft = 0x49,
        LeftRight = 0x4A,
        UpLeftRight = 0x4B,
        LeftDown = 0x4C,
        UpLeftDown = 0x4D,
        LeftDownRight = 0x4E,
        DirectionAll = 0x4F
    }
}