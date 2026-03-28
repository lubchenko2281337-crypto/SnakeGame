using SnakeGame.Models;

namespace SnakeGame.ViewModels;

public sealed class GameModeOption
{
    public required GameMode Mode { get; init; }
    public required string Title { get; init; }
}
