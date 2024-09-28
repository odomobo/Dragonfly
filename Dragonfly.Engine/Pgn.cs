using Dragonfly.Engine.CoreTypes;
using Dragonfly.Engine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dragonfly.Engine
{
    public class Pgn
    {
        public List<(string key, string value)> Headers = new List<(string key, string value)>();
        public List<Position> Positions = new List<Position>();
        public List<Move> Moves = new List<Move>();

        public string ToPgnString(IMoveGenerator moveGenerator)
        {
            var sb = new StringBuilder();
            foreach (var header in Headers)
            {
                sb.AppendLine($"[{header.key} \"{header.value}\"]");
            }
            sb.AppendLine();

            for (int i = 0; i < Moves.Count; i++)
            {
                var position = Positions[i];
                var move = Moves[i];

                // special case for 0, because we might need to print black move number, and we don't prepend space
                if (i == 0)
                {
                    sb.Append(GetPgnMoveNumber(position));
                }
                else if (position.SideToMove == Color.White)
                {
                    sb.Append(" ");
                    sb.Append(GetPgnMoveNumber(position));
                }

                // append move
                sb.Append(" ");
                sb.Append(BoardParsing.SanStringFromMoveBoard(move, position, moveGenerator));
            }
            
            return sb.ToString();
        }

        public static string GetPgnMoveNumber(Position position)
        {
            if (position.SideToMove == Color.White)
            {
                return $"{position.FullMove}.";
            }
            else
            {
                return $"{position.FullMove}...";
            }
        }

        private static readonly Regex PgnHeaderRegex = new Regex(@"^\[(\w+) ""([^""]*)""\]$");
        private static readonly Regex PgnMoveNumberRegex = new Regex(@"^\d+\.(\.\.)?$");
        public static Pgn ParsePgn(IMoveGenerator moveGenerator, string pgn)
        {
            pgn = pgn.Trim();
            var ret = new Pgn();

            var pgnSplit = pgn.Split("\n").Select(l => l.Trim()).ToList();
            var blankLineCount = pgnSplit.Where(l => string.IsNullOrEmpty(l)).Count();
            if (blankLineCount != 1)
            {
                throw new Exception($"Expected exactly 1 blank line in pgn, but instead there are {blankLineCount}");
            }

            var headerLines = pgnSplit.TakeWhile(l => !string.IsNullOrWhiteSpace(l)).ToList();
            // skip 1 extra, because we need to skip the 1 blank line that we find
            var movesLines = pgnSplit.SkipWhile(l => !string.IsNullOrWhiteSpace(l)).Skip(1).ToList();

            foreach (var line in headerLines)
            {
                var match = PgnHeaderRegex.Match(line);
                if (!match.Success)
                {
                    throw new Exception($"Error when parsing PGN header: could not parse \"{line}\"");
                }
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                ret.Headers.Add((key, value));

                if (string.Equals(key, "FEN"))
                {
                    throw new NotImplementedException("Cannot yet parse PGNs which do not start from the initial position");
                }
            }

            // TODO: handle fen starting position
            var position = BoardParsing.GetInitialPosition();
            ret.Positions.Add(position);

            var movesString = string.Join(" ", movesLines);
            var moves = movesString.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            foreach (var sanMove in moves)
            {
                // skip move numbers
                if (PgnMoveNumberRegex.IsMatch(sanMove))
                    continue;

                if (sanMove is "1-0" or "1/2-1/2" or "0-1")
                    continue;

                var move = BoardParsing.GetMoveFromSan(moveGenerator, position, sanMove);
                ret.Moves.Add(move);
                position = position.MakeMove(move);
                ret.Positions.Add(position);
            }

            return ret;
        }
    }
}
