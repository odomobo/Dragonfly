using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.Interfaces
{
    public interface ISearch
    {
        // TODO: add method for extracting statistics during the search? or maybe a callback to send info to the interface?
        (Move move, Statistics statistics) Search(Position position, ITimeStrategy timeStrategy);
    }
}
