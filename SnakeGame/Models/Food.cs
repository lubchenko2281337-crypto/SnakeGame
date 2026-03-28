namespace SnakeGame.Models;

public sealed class Food
{
    public CellPosition Position { get; set; }

    public Food(CellPosition position)
    {
        Position = position;
    }
}
