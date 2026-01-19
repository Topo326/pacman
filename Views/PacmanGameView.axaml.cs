using Avalonia.Controls;
using Avalonia.Input;

namespace PacmanGame.Views;

public partial class PacmanGameView : UserControl
{
    public PacmanGameView()
    {
        InitializeComponent();
    }

    private void Canvas_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is ViewModels.PacmanGameViewModel vm)
        {
            vm.OnKeyDown(e);
        }
    }

    private void Canvas_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Canvas canvas)
        {
            canvas.Focus();
        }
    }
}
