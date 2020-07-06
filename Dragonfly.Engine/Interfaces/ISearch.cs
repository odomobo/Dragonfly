using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonfly.Engine.Interfaces
{
    public interface ISearch
    {
        Move Search(Board b);
    }
}
