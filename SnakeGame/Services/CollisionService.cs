using SnakeGame.Models;

namespace SnakeGame.Services;

public sealed class CollisionService
{
    public bool HitsSelf(CellPosition newHead, IReadOnlyList<CellPosition> bodyAfterMove)
    {
        for (var i = 1; i < bodyAfterMove.Count; i++)
        {
            if (bodyAfterMove[i] == newHead)
                return true;
        }

        return false;
    }

    public bool IsOutOfBounds(CellPosition head, GameField field)
    {
        return head.X < 0 || head.Y < 0 || head.X >= field.WidthCells || head.Y >= field.HeightCells;
    }

    public static CellPosition Wrap(CellPosition head, GameField field)
    {
        var x = Mod(head.X, field.WidthCells);
        var y = Mod(head.Y, field.HeightCells);
        return new CellPosition(x, y);
    }

    private static int Mod(int value, int length)
    {
        var m = value % length;
        return m < 0 ? m + length : m;
    }
}
