using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dragonfly.Engine
{
    // This struct makes strong assumptions about the data layout of Color and PieceType
    public readonly struct Piece
    {
        public static readonly Piece None = new Piece(Color.White, PieceType.None);

        // this is sbyte so piece square array can be small
        private readonly sbyte _data;

        public Color Color => (Color) (_data >> 3);
        public PieceType PieceType => (PieceType) (_data & 0b0111);
        
        public Piece(Color color, PieceType pieceType)
        {
            _data = (sbyte) (((int) color << 3) | (int) pieceType);
        }

        private Piece(sbyte data)
        {
            _data = data;
        }

        public static explicit operator int(Piece piece) => piece._data;
        public static explicit operator Piece(int data) => new Piece((sbyte) data);

        public void Deconstruct(out Color color, out PieceType pieceType)
        {
            color = Color;
            pieceType = PieceType;
        }

        public static bool operator ==(Piece p1, Piece p2)
        {
            return p1._data == p2._data;
        }

        public static bool operator !=(Piece p1, Piece p2)
        {
            return p1._data != p2._data;
        }

        public bool Equals(Piece other)
        {
            return _data == other._data;
        }

        public override bool Equals(object? obj)
        {
            return obj is Piece other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }
    }
}
