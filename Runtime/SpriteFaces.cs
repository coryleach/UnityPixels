using System;

namespace Gameframe.Pixels
{
    [Flags]
    public enum SpriteFaces
    {
        None = 0,
        Front = 1,
        Back = 2,
        Left = 4,
        Right = 8,
        Top = 16,
        Bottom = 32,
        All = -1
    }

    public static class SpriteFacesExtensions
    {
        public static bool Check(this SpriteFaces value, SpriteFaces face)
        {
            return ((value & face) == face && (face != SpriteFaces.None)) || (face == value && face == SpriteFaces.None);
        }
    }
}