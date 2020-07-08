using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonfly.Engine.CoreTypes
{
    public enum Color
    {
        White = 0,
        Black = 1,
    }

    public static class ColorExtensions
    {
        public static int GetPawnDirection(this Color color)
        {
            switch (color)
            {
                case Color.White:
                    return 1;
                case Color.Black:
                    return -1;
                default:
                    throw new Exception($"Cannot get pawn direction on color {color}");
            }
        }

        public static Color Other(this Color color)
        {
            return (Color)(~(int)color & 1);
        }
    }
}
