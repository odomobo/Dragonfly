using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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

        public static ulong CalculateFullHash(Board board)
        {
            ulong hash = 0;
            for (Color color = 0; (int)color < 2; color++)
            {
                for (PieceType pieceType = 0; pieceType < PieceType.Count; pieceType++)
                {
                    var pieces = board.GetPieceBitboard(color, pieceType);
                    while (Bits.TryPopLsb(ref pieces, out var pieceIx))
                    {
                        hash ^= GetPieceHash(color, pieceType, pieceIx);
                    }
                }
            }

            var castlingRights = board.CastlingRights;
            while (Bits.TryPopLsb(ref castlingRights, out var castlingRightIx))
            {
                hash ^= GetCastlingHash(castlingRightIx);
            }

            var enPassant = board.EnPassant;
            while (Bits.TryPopLsb(ref enPassant, out var enPassantIx))
            {
                hash ^= GetEnPassantHash(enPassantIx);
            }

            if (board.SideToMove == Color.White)
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
        public static ulong OtherHashDiff(Board oldBoard, Board newBoard)
        {
            ulong hash = 0;
            var castlingRights = oldBoard.CastlingRights ^ newBoard.CastlingRights;
            while (Bits.TryPopLsb(ref castlingRights, out var castlingRightIx))
            {
                hash ^= GetCastlingHash(castlingRightIx);
            }

            var enPassant = oldBoard.EnPassant ^ newBoard.EnPassant;
            while (Bits.TryPopLsb(ref enPassant, out var enPassantIx))
            {
                hash ^= GetEnPassantHash(enPassantIx);
            }

            Debug.Assert(oldBoard.SideToMove != newBoard.SideToMove);
            hash ^= WhiteSideHash;

            return hash;
        }
    }
}
