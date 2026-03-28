namespace SnakeGame.Models;

public readonly struct CellPosition : IEquatable<CellPosition>
{
    public int X { get; }
    public int Y { get; }

    public CellPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(CellPosition other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is CellPosition other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(CellPosition a, CellPosition b) => a.Equals(b);

    public static bool operator !=(CellPosition a, CellPosition b) => !a.Equals(b);

    public override string ToString() => $"({X},{Y})";
}
