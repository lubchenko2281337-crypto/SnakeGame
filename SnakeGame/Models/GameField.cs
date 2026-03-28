namespace SnakeGame.Models;

public sealed class GameField
{
    public int WidthCells { get; }
    public int HeightCells { get; }

    public GameField(int widthCells, int heightCells)
    {
        WidthCells = widthCells;
        HeightCells = heightCells;
    }
}
