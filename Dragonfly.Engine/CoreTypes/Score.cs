using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dragonfly.Engine.CoreTypes
{
    public readonly struct Score
    {
        // important to not use min short, because this could cause some failures in edge cases where storing to a short and negating
        public static readonly Score MinValue = new Score(-32500);
        public static readonly Score MaxValue = new Score(32500);

        private readonly int _value;
        public int Value => _value;

        public Score(int value)
        {
            _value = value;
        }

        public int GetValue()
        {
            return _value;
        }

        // Score for the side being mated; relative to the side being mated
        public static Score GetMateScore(int gamePly)
        {
            return -32000 + gamePly;
        }

        // Score for the side being mated; absolute
        public static Score GetMateScore(Color color, int gamePly)
        {
            if (color == Color.White)
                return GetMateScore(gamePly);
            else
                return -GetMateScore(gamePly);
        }

        public bool IsMateScore()
        {
            if (_value > -30000 && _value < 30000)
                return false;
            else
                return true;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public static implicit operator Score(int score) => new Score(score);
        public static explicit operator int(Score score) => score._value;

        #region Operators

        public static Score operator +(Score a, Score b) => a._value + b._value;
        public static Score operator -(Score a, Score b) => a._value - b._value;
        public static Score operator *(Score a, Score b) => a._value * b._value;
        public static Score operator /(Score a, Score b) => a._value / b._value;
        public static Score operator -(Score a) => -a._value;
        public static bool operator >(Score a, Score b) => a._value > b._value;
        public static bool operator <(Score a, Score b) => a._value < b._value;
        public static bool operator >=(Score a, Score b) => a._value >= b._value;
        public static bool operator <=(Score a, Score b) => a._value <= b._value;
        public static bool operator ==(Score a, Score b) => a._value == b._value;
        public static bool operator !=(Score a, Score b) => a._value != b._value;

        #endregion
    }
}
