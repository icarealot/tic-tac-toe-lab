using System;

namespace TicTacToeLab.Runtime
{
    public readonly struct CellCoordinate : IEquatable<CellCoordinate>
    {
        public int Row { get; }
        public int Column { get; }

        public CellCoordinate(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public bool IsWithin(int dimension)
        {
            return Row >= 0 && Row < dimension && Column >= 0 && Column < dimension;
        }

        public bool Equals(CellCoordinate other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object otherObject)
        {
            return otherObject is CellCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        public static bool operator ==(CellCoordinate left, CellCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CellCoordinate left, CellCoordinate right)
        {
            return !left.Equals(right);
        }
    }
}
