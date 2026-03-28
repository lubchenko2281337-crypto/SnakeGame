using System.Windows;
using System.Windows.Input;
using SnakeGame.Models;
using SnakeGame.ViewModels;

namespace SnakeGame;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new GameViewModel();
        Loaded += (_, _) => Focus();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (DataContext is not GameViewModel vm)
            return;

        var dir = e.Key switch
        {
            Key.Up => Direction.Up,
            Key.Down => Direction.Down,
            Key.Left => Direction.Left,
            Key.Right => Direction.Right,
            _ => (Direction?)null
        };

        if (dir is not { } d)
            return;

        if (vm.Phase != GamePhase.Playing)
            return;

        vm.TrySetDirection(d);
        e.Handled = true;
    }
}