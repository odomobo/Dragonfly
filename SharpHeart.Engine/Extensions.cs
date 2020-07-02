using System;
using System.Collections.Generic;
using System.Text;

namespace SharpHeart.Engine
{
    static class Extensions
    {
        public static T GetOrDefault<T>(this T[] array, int index) where T : struct
        {
            if (index < array.Length)
                return array[index];
            else
                return default;
        }
    }
}
