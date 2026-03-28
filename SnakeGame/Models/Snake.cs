namespace SnakeGame.Models;

public sealed class Snake
{
    public List<CellPosition> Segments { get; }

    public Snake(IEnumerable<CellPosition> initialSegments)
    {
        Segments = new List<CellPosition>(initialSegments);
    }

    public CellPosition Head => Segments[0];
}
