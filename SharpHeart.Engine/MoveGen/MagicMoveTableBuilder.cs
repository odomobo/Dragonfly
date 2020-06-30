using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SharpHeart.Engine.MoveGen
{
    public class MagicMoveTableBuilder
    {
        private readonly SortedDictionary<int, MagicMoveTable.Info> _infos = new SortedDictionary<int, MagicMoveTable.Info>();
        public bool _magicFailed = false;
        private readonly MagicFinder _magicFinder;
        private readonly int _tableIndexBits;

        public MagicMoveTableBuilder(MagicFinder magicFinder, int tableIndexBits)
        {
            _magicFinder = magicFinder;
            _tableIndexBits = tableIndexBits;
        }

        public void Add(int ix, MagicMoveTable.Info info)
        {
            Debug.Assert(ix >= 0 && ix < 64);

            if (!MagicFinder.CheckMagic(info.Mask, info.Magic, _tableIndexBits))
            {
                // we don't need to warn that magic was invalid if they didn't provide a magic
                if (info.Magic != 0)
                    _magicFailed = true;

                info.Magic = _magicFinder.FindMagic(info.Mask, _tableIndexBits);
            }

            _infos.Add(ix, info);
        }

        public MagicMoveTable Build()
        {
            Debug.Assert(_infos.Count == 64);
            if (_magicFailed)
            {
                BoardParsing.Dump("***** Invalid magic provided to magic move table builder; need to recalc magic tables.");
            }
            return new MagicMoveTable(_infos.Values.ToArray(), _tableIndexBits);
        }
    }
}
