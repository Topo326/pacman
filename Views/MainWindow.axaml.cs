using Avalonia.Controls;
using Avalonia.Input;
using PacmanGame.ViewModels;

namespace PacmanGame.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += MainWindow_KeyDown;
    }

    private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is MainWindowViewModel mw && mw.CurrentViewModel is GameViewModel g)
        {
            g.OnKeyDown(e.Key);
        }
    }
}