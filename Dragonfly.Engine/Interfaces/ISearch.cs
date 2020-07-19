using System;
using System.Collections.Generic;
using System.Text;
using Dragonfly.Engine.CoreTypes;

namespace Dragonfly.Engine.Interfaces
{
    public interface ISearch
    {
        (Move move, Statistics statistics) Search(Position position, ITimeStrategy timeStrategy, Statistics.PrintInfoDelegate printInfoDelegate);
    }
}
