using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia.VisualTree;

namespace Dragonfly.ToolsGui
{
    public static class Extensions
    {
        // TODO: remove, as not needed
        public static T FindAncestor<T>(this Visual visual) where T : Visual
        {
            while (true)
            {
                visual = visual.GetVisualParent();
                if (visual == null)
                    throw new InvalidOperationException($"Could not find ancestor of type {typeof(T).Name}");

                if (visual is T)
                    return (T)visual;
            }
        }
    }
}
