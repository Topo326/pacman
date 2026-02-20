using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PacmanGame.ViewModels;

namespace PacmanGame.Views;

public partial class PacmanGameView : UserControl
{
    public PacmanGameView()
    {
        InitializeComponent();
    }
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (DataContext is PacmanGameViewModel vm)
        {
            vm.HandleInput(e.Key);
        }
    }
}
