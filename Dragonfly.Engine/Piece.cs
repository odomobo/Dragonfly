using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonfly.Engine
{
    // This struct makes strong assumptions about the data layout of Color and PieceType
    public readonly struct Piece
    {
        public static readonly Piece None = new Piece(Color.White, PieceType.None);

        // TODO: should this be a byte?
        private readonly byte _data;

        public Color Color => (Color) (_data >> 3);
        public PieceType PieceType => (PieceType) (_data & 0b0111);
        
        public Piece(Color color, PieceType pieceType)
        {
            _data = (byte) (((int) color << 3) | (int) pieceType);
        }

        private Piece(byte data)
        {
            _data = data;
        }

        public static explicit operator int(Piece piece) => piece._data;
        public static explicit operator Piece(int data) => new Piece((byte) data);


        public void Deconstruct(out Color color, out PieceType pieceType)
        {
            color = Color;
            pieceType = PieceType;
        }
    }
}
