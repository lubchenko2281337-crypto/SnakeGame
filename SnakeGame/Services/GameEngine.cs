using SnakeGame.Models;

namespace SnakeGame.Services;

public enum TickOutcome
{
    Ok,
    GameOver
}

public sealed class GameEngine
{
    private readonly CollisionService _collision = new();
    private readonly Random _random = new();

    public TickResult Tick(
        Snake snake,
        Food food,
        GameField field,
        GameMode mode,
        Direction direction)
    {
        var head = snake.Head;
        var delta = DirectionToDelta(direction);
        var newHead = new CellPosition(head.X + delta.X, head.Y + delta.Y);

        if (mode == GameMode.ClassicWalls)
        {
            if (_collision.IsOutOfBounds(newHead, field))
                return new TickResult(TickOutcome.GameOver, 0, false, food.Position);
        }
        else
        {
            newHead = CollisionService.Wrap(newHead, field);
        }

        var ateFood = newHead == food.Position;
        var scoreDelta = ateFood ? 1 : 0;

        var newBody = new List<CellPosition>(snake.Segments);
        if (!ateFood)
            newBody.RemoveAt(newBody.Count - 1);
        newBody.Insert(0, newHead);

        if (_collision.HitsSelf(newHead, newBody))
            return new TickResult(TickOutcome.GameOver, 0, false, food.Position);

        snake.Segments.Clear();
        foreach (var s in newBody)
            snake.Segments.Add(s);

        CellPosition foodPosition = food.Position;
        if (ateFood)
            foodPosition = PlaceFood(snake, field, food.Position);

        return new TickResult(TickOutcome.Ok, scoreDelta, ateFood, foodPosition);
    }

    public CellPosition PlaceFood(Snake snake, GameField field, CellPosition? avoidDuplicate = null)
    {
        var maxAttempts = field.WidthCells * field.HeightCells * 2;
        var occupied = new HashSet<CellPosition>(snake.Segments);

        for (var i = 0; i < maxAttempts; i++)
        {
            var x = _random.Next(field.WidthCells);
            var y = _random.Next(field.HeightCells);
            var candidate = new CellPosition(x, y);
            if (occupied.Contains(candidate))
                continue;
            return candidate;
        }

        return avoidDuplicate ?? new CellPosition(0, 0);
    }

    public static bool IsOpposite(Direction current, Direction next)
    {
        return (current, next) switch
        {
            (Direction.Up, Direction.Down) => true,
            (Direction.Down, Direction.Up) => true,
            (Direction.Left, Direction.Right) => true,
            (Direction.Right, Direction.Left) => true,
            _ => false
        };
    }

    public static (int X, int Y) DirectionToDelta(Direction direction)
    {
        return direction switch
        {
            Direction.Up => (0, -1),
            Direction.Down => (0, 1),
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            _ => (0, 0)
        };
    }
}

public readonly struct TickResult
{
    public TickOutcome Outcome { get; }
    public int ScoreDelta { get; }
    public bool AteFood { get; }
    public CellPosition FoodPosition { get; }

    public TickResult(TickOutcome outcome, int scoreDelta, bool ateFood, CellPosition foodPosition)
    {
        Outcome = outcome;
        ScoreDelta = scoreDelta;
        AteFood = ateFood;
        FoodPosition = foodPosition;
    }
}
