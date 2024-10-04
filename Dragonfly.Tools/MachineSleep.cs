using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.Tools
{
    public static class MachineSleep
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SetThreadExecutionState(uint esFlags);
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;

        public static void KeepAwake()
        {
            // we currently don't have logic for mac or linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED);
            }
        }

        public static void AllowSleep()
        {
            // we currently don't have logic for mac or linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetThreadExecutionState(ES_CONTINUOUS);
            }
        }
    }
}
