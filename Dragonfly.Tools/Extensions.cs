using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.Tools
{
    public static class Extensions
    {
        public static string ToShortString(this decimal self)
        {
            if (self < 1000)
                return $"{self:0}";
            
            if (self < 10_000)
                return $"{self/1000m:0.##}k";

            if (self < 100_000)
                return $"{self/1000m:0.#}k";

            if (self < 1_000_000)
                return $"{self/1000m:0}k";

            if (self < 10_000_000)
                return $"{self/1_000_000m:0.##}m";

            if (self < 100_000_000)
                return $"{self/1_000_000m:0.#}m";

            if (self < 1_000_000_000)
                return $"{self/1_000_000m:0}m";

            if (self < 10_000_000_000)
                return $"{self / 1_000_000_000m:0.##}b";

            if (self < 100_000_000_000)
                return $"{self / 1_000_000_000m:0.#}b";

            // we shouldn't need more than 1 trillion
            //if (self < 1_000_000_000_000)
            return $"{self / 1_000_000_000m:0}b";
        }

        public static string ToShortString(this int self)
        {
            return ((decimal)self).ToShortString();
        }

        public static string ToShortString(this double self)
        {
            return ((decimal)self).ToShortString();
        }
    }
}
