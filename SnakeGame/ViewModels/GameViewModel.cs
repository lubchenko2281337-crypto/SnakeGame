using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using SnakeGame.Helpers;
using SnakeGame.Models;
using SnakeGame.Services;

namespace SnakeGame.ViewModels;

public sealed class GameViewModel : ViewModelBase
{
    private const int BaseIntervalMs = 180;
    private const int MinIntervalMs = 85;
    private const int IntervalStepMs = 12;
    private const int FoodsPerSpeedStep = 4;

    private readonly GameEngine _engine = new();
    private readonly GameField _field;
    private readonly HighScoreStore _highScoreStore = new();
    private readonly DispatcherTimer _timer;

    private Snake _snake = null!;
    private Food _food = null!;
    private Direction _currentDirection = Direction.Right;
    private Direction _pendingDirection = Direction.Right;
    private GameMode _gameMode = GameMode.ClassicWalls;
    private GamePhase _phase = GamePhase.Idle;
    private int _score;
    private int _bestScore;
    private int _totalFoodEaten;
    private CellPosition _foodPosition;

    public GameViewModel()
    {
        _field = new GameField(20, 20);
        _bestScore = _highScoreStore.Load();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(BaseIntervalMs) };
        _timer.Tick += (_, _) => OnGameTick();

        SnakeSegments = new ObservableCollection<SnakeSegmentDisplay>();
        _foodPosition = new CellPosition(0, 0);

        StartGameCommand = new RelayCommand(StartGame, () => _phase is GamePhase.Idle or GamePhase.GameOver);
        PauseCommand = new RelayCommand(TogglePause, () => _phase is GamePhase.Playing or GamePhase.Paused);
        RestartCommand = new RelayCommand(Restart);
        SetDirectionCommand = new RelayCommand(p => TrySetDirection(ParseDirection(p)), _ => _phase == GamePhase.Playing);
    }

    public int CellSize => 20;

    public int FieldWidthCells => _field.WidthCells;

    public int FieldHeightCells => _field.HeightCells;

    public int CanvasPixelWidth => _field.WidthCells * CellSize;

    public int CanvasPixelHeight => _field.HeightCells * CellSize;

    public GameModeOption[] GameModeOptions { get; } =
    {
        new GameModeOption { Mode = GameMode.ClassicWalls, Title = "Классика (стены)" },
        new GameModeOption { Mode = GameMode.InfiniteField, Title = "Бесконечное поле" }
    };

    public ObservableCollection<SnakeSegmentDisplay> SnakeSegments { get; }

    public CellPosition FoodPosition
    {
        get => _foodPosition;
        private set
        {
            if (!SetField(ref _foodPosition, value))
                return;
            OnPropertyChanged(nameof(FoodLeft));
            OnPropertyChanged(nameof(FoodTop));
        }
    }

    public double FoodLeft => FoodPosition.X * CellSize;

    public double FoodTop => FoodPosition.Y * CellSize;

    public int Score
    {
        get => _score;
        private set => SetField(ref _score, value);
    }

    public int BestScore
    {
        get => _bestScore;
        private set => SetField(ref _bestScore, value);
    }

    public GameMode GameMode
    {
        get => _gameMode;
        set => SetField(ref _gameMode, value);
    }

    public GamePhase Phase
    {
        get => _phase;
        private set
        {
            if (!SetField(ref _phase, value))
                return;
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(CanChangeSettings));
            OnPropertyChanged(nameof(PauseButtonText));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string StatusText => Phase switch
    {
        GamePhase.Idle => "Готов к игре",
        GamePhase.Playing => "Идёт игра",
        GamePhase.Paused => "Пауза",
        GamePhase.GameOver => "Проигрыш",
        _ => ""
    };

    public bool CanChangeSettings => Phase is GamePhase.Idle or GamePhase.GameOver;

    public string PauseButtonText => Phase == GamePhase.Paused ? "Продолжить" : "Пауза";

    public ICommand StartGameCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand RestartCommand { get; }
    public ICommand SetDirectionCommand { get; }

    private static Direction ParseDirection(object? parameter)
    {
        return parameter switch
        {
            Direction d => d,
            string s when Enum.TryParse<Direction>(s, true, out var dir) => dir,
            _ => Direction.Right
        };
    }

    private void StartGame()
    {
        _totalFoodEaten = 0;
        Score = 0;
        Phase = GamePhase.Playing;
        _currentDirection = Direction.Right;
        _pendingDirection = Direction.Right;

        var midY = _field.HeightCells / 2;
        var midX = _field.WidthCells / 2;
        _snake = new Snake(new[]
        {
            new CellPosition(midX, midY),
            new CellPosition(midX - 1, midY),
            new CellPosition(midX - 2, midY)
        });

        _food = new Food(_engine.PlaceFood(_snake, _field));
        FoodPosition = _food.Position;
        SyncSnakeToUi();
        ApplyTimerInterval();
        _timer.Start();
    }

    private void Restart()
    {
        _timer.Stop();
        StartGame();
    }

    private void TogglePause()
    {
        if (_phase == GamePhase.Playing)
        {
            _timer.Stop();
            Phase = GamePhase.Paused;
        }
        else if (_phase == GamePhase.Paused)
        {
            Phase = GamePhase.Playing;
            ApplyTimerInterval();
            _timer.Start();
        }
    }

    public void TrySetDirection(Direction next)
    {
        if (GameEngine.IsOpposite(_pendingDirection, next))
            return;
        _pendingDirection = next;
    }

    private void OnGameTick()
    {
        if (_phase != GamePhase.Playing)
            return;

        if (!GameEngine.IsOpposite(_currentDirection, _pendingDirection))
            _currentDirection = _pendingDirection;

        var result = _engine.Tick(_snake, _food, _field, _gameMode, _currentDirection);

        if (result.Outcome == TickOutcome.GameOver)
        {
            _timer.Stop();
            Phase = GamePhase.GameOver;
            _highScoreStore.SaveIfBetter(Score);
            BestScore = _highScoreStore.Load();
            return;
        }

        Score += result.ScoreDelta;
        if (result.AteFood)
        {
            _totalFoodEaten++;
            _food.Position = result.FoodPosition;
            FoodPosition = result.FoodPosition;
            ApplyTimerInterval();
        }

        SyncSnakeToUi();
    }

    private void ApplyTimerInterval()
    {
        var steps = _totalFoodEaten / FoodsPerSpeedStep;
        var ms = Math.Max(MinIntervalMs, BaseIntervalMs - steps * IntervalStepMs);
        _timer.Interval = TimeSpan.FromMilliseconds(ms);
    }

    private void SyncSnakeToUi()
    {
        SnakeSegments.Clear();
        foreach (var s in _snake.Segments)
        {
            SnakeSegments.Add(new SnakeSegmentDisplay
            {
                Left = s.X * CellSize,
                Top = s.Y * CellSize
            });
        }
    }

}
