using System;
using System.Collections.ObjectModel;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PacmanGame.Models;
using PacmanGame.Services;

namespace PacmanGame.ViewModels;

public partial class PacmanGameViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;
    private readonly IGameService _game;
    private readonly DispatcherTimer _gameLoop;

    [ObservableProperty] private int _score;
    [ObservableProperty] private int _lives = 3;
    [ObservableProperty] private double _dotOffset = 8;
    [ObservableProperty] private double _mapOffsetX = 0;
    [ObservableProperty] private double _mapOffsetY = 0;

    public ObservableCollection<EntityViewModel> GameEntities { get; } = new();

    public PacmanGameViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _game = new GameService();
        _gameLoop = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _gameLoop.Tick += OnGameTick;
        StartGame();
    }

    private void StartGame()
    {
        _game.Initialize();
        SyncViewModels();
        _gameLoop.Start();
    }

    private void SyncViewModels()
    {
        GameEntities.Clear();

        // Add Walls
        int ts = GameMap.TileSize;
        for (int r = 0; r < _game.Map.Rows; r++)
        for (int c = 0; c < _game.Map.Cols; c++)
        {
            if (_game.Map.IsWall(r, c))
            {
                GameEntities.Add(new WallViewModel { X = c * ts, Y = r * ts });
            }
        }

        // Add Dots
        foreach (var dot in _game.Dots)
            GameEntities.Add(new DotViewModel { X = dot.x - 8 + DotOffset, Y = dot.y - 8 + DotOffset }); 

        foreach (var ghost in _game.Ghosts)
        {
            var vm = new GhostEntityViewModel 
            { 
                X = ghost.X, 
                Y = ghost.Y,
                Type = ghost.Type,
                IsActive = ghost.IsActive
            };
            vm.LoadSprite(GetGhostSprite(ghost.Type));
            GameEntities.Add(vm);
        }

        var playerVm = new PlayerEntityViewModel { X = _game.Player.X, Y = _game.Player.Y };
        playerVm.LoadSprite("PacMan.gif");
        GameEntities.Add(playerVm);
    }

    public void HandleInput(Key key)
    {
        var direction = key switch
        {
            Key.Up => MovementDirection.Up,
            Key.Down => MovementDirection.Down,
            Key.Left => MovementDirection.Left,
            Key.Right => MovementDirection.Right,
            _ => MovementDirection.None
        };
        
        if (direction != MovementDirection.None)
            _game.HandleInput(direction);
    }

    private void OnGameTick(object? sender, EventArgs e)
    {
        _game.Update();
        UpdateView();
    }

    private void UpdateView()
    {
        Score = _game.State.Score;
        Lives = _game.State.Lives;

        int ghostIndex = 0;
        
        foreach (var entity in GameEntities)
        {
            switch (entity)
            {
                case PlayerEntityViewModel player:
                    player.X = _game.Player.X;
                    player.Y = _game.Player.Y;
                    break;
                    
                case GhostEntityViewModel ghost when ghostIndex < _game.Ghosts.Count:
                    var g = _game.Ghosts[ghostIndex++];
                    ghost.X = g.X;
                    ghost.Y = g.Y;
                    ghost.IsActive = g.IsActive;
                    break;
            }
        }

        for (int i = GameEntities.Count - 1; i >= 0; i--)
        {
            if (GameEntities[i] is not DotViewModel dot) continue;
            
            bool exists = false;
            foreach (var d in _game.Dots)
            {
                // Calculate expected visual position for this model dot
                double expectedX = d.x - 8 + DotOffset;
                double expectedY = d.y - 8 + DotOffset;
                
                if (Math.Abs(expectedX - dot.X) < 1 && Math.Abs(expectedY - dot.Y) < 1)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists) GameEntities.RemoveAt(i);
        }
    }

    private static string GetGhostSprite(GhostType type) => type switch
    {
        GhostType.Blinky => "Blinky_Down.gif",
        GhostType.Pinky => "Pinky_Down.gif",
        GhostType.Inky => "Inky_Down.gif",
        GhostType.Clyde => "Clyde_Down.gif",
        _ => "Blinky_Down.gif"
    };
}
