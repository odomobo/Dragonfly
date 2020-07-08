using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Dragonfly.Engine.CoreTypes;
using MersenneTwister;

namespace Dragonfly.Engine
{
    public static class ZobristHashing
    {
        private static readonly ulong[] PieceHashes;
        private static readonly ulong[] EnPassantHashes;
        private static readonly ulong[] CastlingHashes;
        public static readonly ulong WhiteSideHash;

        static ZobristHashing()
        {
            var random = new MTRandom(0);
            PieceHashes = new ulong[2 * (int)PieceType.Count * 64];
            Fill(PieceHashes, random);
            EnPassantHashes = new ulong[64];
            Fill(EnPassantHashes, random);
            CastlingHashes = new ulong[64];
            Fill(CastlingHashes, random);
            WhiteSideHash = random.genrand_int64();
        }

        private static void Fill(ulong[] array, MTRandom random)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = random.genrand_int64();
            }
        }

        public static ulong GetPieceHash(Color color, PieceType pieceType, int pieceIx)
        {
            return PieceHashes[((int) color * (int) PieceType.Count * 64) + ((int) pieceType * 64) + pieceIx];
        }

        public static ulong GetEnPassantHash(int enPassantIx)
        {
            return EnPassantHashes[enPassantIx];
        }

        public static ulong GetCastlingHash(int castlingRightIx)
        {
            return CastlingHashes[castlingRightIx];
        }

        public static ulong CalculateFullHash(Position position)
        {
            ulong hash = 0;
            for (Color color = 0; (int)color < 2; color++)
            {
                for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
                {
                    var pieces = position.GetPieceBitboard(color, pieceType);
                    while (Bits.TryPopLsb(ref pieces, out var pieceIx))
                    {
                        hash ^= GetPieceHash(color, pieceType, pieceIx);
                    }
                }
            }

            var castlingRights = position.CastlingRights;
            while (Bits.TryPopLsb(ref castlingRights, out var castlingRightIx))
            {
                hash ^= GetCastlingHash(castlingRightIx);
            }

            var enPassant = position.EnPassant;
            while (Bits.TryPopLsb(ref enPassant, out var enPassantIx))
            {
                hash ^= GetEnPassantHash(enPassantIx);
            }

            if (position.SideToMove == Color.White)
                hash ^= WhiteSideHash;

            return hash;
        }

        // TODO: rename?
        public static ulong CalculatePieceBitboardHashDiff(ulong pieces, Color color, PieceType pieceType)
        {
            ulong hash = 0;
            while (Bits.TryPopLsb(ref pieces, out var pieceIx))
            {
                hash ^= GetPieceHash(color, pieceType, pieceIx);
            }

            return hash;
        }

        // TODO: rename to something better
        public static ulong OtherHashDiff(Position oldPosition, Position newPosition)
        {
            ulong hash = 0;
            var castlingRights = oldPosition.CastlingRights ^ newPosition.CastlingRights;
            while (Bits.TryPopLsb(ref castlingRights, out var castlingRightIx))
            {
                hash ^= GetCastlingHash(castlingRightIx);
            }

            var enPassant = oldPosition.EnPassant ^ newPosition.EnPassant;
            while (Bits.TryPopLsb(ref enPassant, out var enPassantIx))
            {
                hash ^= GetEnPassantHash(enPassantIx);
            }

            Debug.Assert(oldPosition.SideToMove != newPosition.SideToMove);
            hash ^= WhiteSideHash;

            return hash;
        }
    }
}
