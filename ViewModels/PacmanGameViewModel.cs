using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PacmanGame.Models;
using PacmanGame.Services;

namespace PacmanGame.ViewModels;

/// <summary>
/// ViewModel para la pantalla de juego de Pacman.
/// </summary>
public partial class PacmanGameViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;
    private readonly IGameService _game;
    private readonly DispatcherTimer _gameLoop;

    [ObservableProperty] private int _score;
    [ObservableProperty] private int _highScore;
    [ObservableProperty] private int _lives = 3;
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

        // Agregar Paredes
        int ts = GameConstants.TileSize;
        for (int r = 0; r < _game.Map.Rows; r++)
        for (int c = 0; c < _game.Map.Cols; c++)
        {
            if (_game.Map.IsWall(r, c))
            {
                GameEntities.Add(new WallViewModel { X = c * ts, Y = r * ts });
            }
        }

        // Agregar Puntos y Súper Píldoras
        foreach (var dot in _game.Dots)
        {
            if (dot.isPower)
                GameEntities.Add(new PowerPillViewModel { X = dot.x, Y = dot.y });
            else
                GameEntities.Add(new DotViewModel { X = dot.x, Y = dot.y });
        }

        // Agregar Fantasmas
        foreach (var ghost in _game.Ghosts)
        {
            var vm = new GhostEntityViewModel 
            { 
                X = ghost.X, 
                Y = ghost.Y,
                Type = ghost.Type,
                IsActive = ghost.IsActive
            };
            vm.LoadSprite(GetGhostSprite(ghost.Type, false));
            GameEntities.Add(vm);
        }

        // Agregar Jugador
        var playerVm = new PlayerEntityViewModel { X = _game.Player.X, Y = _game.Player.Y };
        playerVm.LoadSprite(GetPlayerSprite(_game.Player.CurrentDirection));
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
        HighScore = _game.State.HighScore;
        Lives = _game.State.Lives;

        int ghostIndex = 0;
        bool isFrightened = _game.State.IsFrightenedMode;
        
        foreach (var entity in GameEntities)
        {
            switch (entity)
            {
                case PlayerEntityViewModel player:
                    player.X = _game.Player.X;
                    player.Y = _game.Player.Y;
                    player.IsDead = _game.Player.IsDead;
                    if (!player.IsDead) player.LoadSprite(GetPlayerSprite(_game.Player.CurrentDirection));
                    break;
                    
                case GhostEntityViewModel ghost when ghostIndex < _game.Ghosts.Count:
                    var g = _game.Ghosts[ghostIndex++];
                    ghost.X = g.X;
                    ghost.Y = g.Y;
                    ghost.IsActive = g.IsActive;
                    ghost.IsFrightened = isFrightened;
                    ghost.LoadSprite(GetGhostSprite(g.Type, isFrightened));
                    break;
            }
        }

        // Si el backend tiene significativamente más puntos que la vista, significa que el estado se reinició (vidas a 0 o nivel nuevo).
        // Forzamos un redibujado de todos los elementos para que reaparezcan en pantalla.
        var totalDots = _game.Dots;
        var viewDotsCount = GameEntities.Count(e => e is DotViewModel || e is PowerPillViewModel);
        if (totalDots.Count > viewDotsCount)
        {
            SyncViewModels();
            return;
        }

        // Eliminar puntos recolectados
        var currentDots = _game.Dots;
        for (int i = GameEntities.Count - 1; i >= 0; i--)
        {
            var entity = GameEntities[i];
            if (entity is DotViewModel || entity is PowerPillViewModel)
            {
                bool stillExists = currentDots.Any(d => Math.Abs(d.x - entity.X) < 1 && Math.Abs(d.y - entity.Y) < 1);
                if (!stillExists)
                {
                    GameEntities.RemoveAt(i);
                }
            }
        }
    }

    private static string GetGhostSprite(GhostType type, bool isFrightened)
    {
        if (isFrightened) return "Blue_Ghost.gif";
        
        return type switch
        {
            GhostType.Blinky => "Blinky_Down.gif",
            GhostType.Pinky => "Pinky_Down.gif",
            GhostType.Inky => "Inky_Down.gif",
            GhostType.Clyde => "Clyde_Down.gif",
            _ => "Blinky_Down.gif"
        };
    }

    private static string GetPlayerSprite(MovementDirection dir) => "PacMan.gif";
}
