using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.Interfaces
{
    public interface ISearch
    {
        // TODO: how to control depth, or whatever else?
        Move Search(Position position);
    }
}
