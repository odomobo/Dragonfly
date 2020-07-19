using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonfly.Engine
{
    public sealed class SynchronizedFlag
    {
        private volatile bool _isSet = false;

        public void Set()
        {
            _isSet = true;
        }

        public void Clear()
        {
            _isSet = false;
        }

        public bool IsSet() => _isSet;
    }
}
