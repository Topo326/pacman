using System;
using System.Collections.ObjectModel;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PacmanGame.Models;

namespace PacmanGame.ViewModels;

public partial class PacmanGameViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;
    private DispatcherTimer _gameLoop;
    
    private EntityViewModel _pacman = null!; 

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _lives = 3;

    public ObservableCollection<EntityViewModel> GameEntities { get; } = new();

    public PacmanGameViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _gameLoop = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _gameLoop.Tick += GameLoop_Tick;
        
        StartGame();
    }

    private void StartGame()
    {
        GameEntities.Clear();


        _pacman = new EntityViewModel { X = 224, Y = 376 };
        _pacman.LoadSprite("Pacman_animate.gif"); 
        
        GameEntities.Add(_pacman);

        var ghost = new EntityViewModel { X = 200, Y = 200 };
        ghost.LoadSprite("Blinky_Left.gif");
        GameEntities.Add(ghost);

        _gameLoop.Start();
    }

    public void HandleInput(Key key)
    {
        if (_pacman == null) return;

        switch (key)
        {
            case Key.Up:    _pacman.Y -= 10; break;
            case Key.Down:  _pacman.Y += 10; break;
            case Key.Left:  _pacman.X -= 10; break;
            case Key.Right: _pacman.X += 10; break;
        }
    }

    private void GameLoop_Tick(object? sender, EventArgs e)
    {
        foreach(var entity in GameEntities)
        {
            entity.Move();
        }
    }
}