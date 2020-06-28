﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine
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
            switch (color)
            {
                case Color.White:
                    return Color.Black;
                case Color.Black:
                    return Color.White;
                default:
                    throw new Exception($"Cannot switch color {color}");
            }
        }
    }
}
