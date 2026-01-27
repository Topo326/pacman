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
    [ObservableProperty] private double _dotOffset = 8; // Configurable offset for Dots only
    [ObservableProperty] private double _mapOffsetX = 0; // Configurable offset for ENTIRE GRID (Walls, Dots, Ghosts, Player)
    [ObservableProperty] private double _mapOffsetY = 0; // Configurable offset for ENTIRE GRID

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
            // Original logic was: dot.x is center? No, dot.x in GameService is (c*ts + 8).
            // If dot.x is already offset by 8, and we want to adjust it.
            // GameService: _dots.Add((c * ts + 8, r * ts + 8));
            // So dot.x is 8px from top-left of tile.
            // If we want configurable offset, we should probably use the raw tile position or adjust relative to the 8.
            // Let's assume DotOffset replaces the '8'.
            // But GameService stores the absolute position.
            // If we want to shift it visually, we can just add (DotOffset - 8) to the stored position.
            // Or better: Re-calculate based on tile if possible, but we only have list of coords.
            // Let's assume DotOffset is the shift from the tile corner.
            // But we don't know the tile corner easily from just x/y without modulo.
            // Wait, GameService initializes dots as `c * ts + 8`.
            // So if DotOffset is 9, we want `c * ts + 9`.
            // So we add `DotOffset - 8` to the current `dot.x`.
            // Let's do that.
            
        // Actually, let's just use the stored position and add a visual offset if needed.
        // The user said: "Cambia el offset del DotViewModel a +8 en lugar de +9".
        // Currently GameService uses +8.
        // If user wants to change it visually, we can apply a delta.
        // Let's assume DotOffset is the DESIRED offset from tile start.
        // Since we don't have tile start, let's assume the stored position is correct for logic (hitbox),
        // but for visual we might want to shift it.
        // However, the user said: "GameEntities.Add(new DotViewModel { X = posX + 9, Y = posY + 9 });"
        // In my code, `dot.x` comes from `GameService` which uses +8.
        // So `dot.x` is `TileX + 8`.
        // If I want `TileX + DotOffset`, I can do `dot.x - 8 + DotOffset`.
        
        // Let's iterate dots again properly.
        // Actually, `SyncViewModels` is called once at start.
        // But `UpdateView` removes dots.
        // If I change `DotOffset` at runtime, I might need to refresh `GameEntities`.
        // For now, let's just apply it here.
        
        // Re-reading SyncViewModels in original code:
        // foreach (var dot in _game.Dots) GameEntities.Add(new DotViewModel { X = dot.x, Y = dot.y });
        // So it was using +8 (from GameService).
        // User wants configurable.
        
        // Let's do:
        // GameEntities.Add(new DotViewModel { X = dot.x - 8 + DotOffset, Y = dot.y - 8 + DotOffset });

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
