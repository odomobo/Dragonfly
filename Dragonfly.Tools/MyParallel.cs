using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dragonfly.Tools
{
    public static class MyParallel
    {
        public static List<TResult> Map<TSource, TResult>(IList<TSource> source, ParallelOptions parallelOptions, Func<TSource, TResult> body)
        {
            // first, add results to concurrent dictionary... concurrently
            var results = new ConcurrentDictionary<int, TResult>();
            Parallel.For(0, source.Count, parallelOptions, i =>
            {
                var result = body(source[i]);
                results[i] = result;
            });

            // then transform to a list and return
            var ret = new List<TResult>();
            for (int i = 0; i < source.Count; i++)
            {
                ret.Add(results[i]);
            }

            return ret;
        }
    }
}
