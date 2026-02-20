using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PacmanGame.Models;
using PacmanGame.Services;
using System.Diagnostics;

namespace PacmanGame.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;
    private readonly IGameService _game;
    private readonly System.Timers.Timer _tick;
    private readonly ScoreBoard _scoreBoard = new();
    private Process? _audioProcess;

    [ObservableProperty] private int _playerX;
    [ObservableProperty] private int _playerY;
    [ObservableProperty] private int _score;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _playerSprite = "Assets/PacMan_Default.png";

    public ObservableCollection<GhostData> GhostData { get; } = new();
    public ObservableCollection<Tile> Map { get; } = new();
    public IReadOnlyList<int> TopScores => _scoreBoard.TopScores;

    public GameViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _main = mainWindowViewModel;
        _game = new GameService();
        _tick = new System.Timers.Timer(120);
        _tick.Elapsed += OnTick;
    }

    [RelayCommand]
    public void StartGame()
    {
        PlayStartSound();
        _game.Initialize();
        UpdateGhostData();
        
        Score = 0;
        PlayerX = (int)_game.Player.X;
        PlayerY = (int)_game.Player.Y;
        
        IsRunning = true;
        _tick.Start();
    }

    private void PlayStartSound()
    {
        _audioProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "aplay",
                Arguments = "Assets/pacman_beginning.wav",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        _audioProcess.Start();
    }

    [RelayCommand]
    public void StopGame()
    {
        _tick.Stop();
        IsRunning = false;
    }

    [RelayCommand]
    public void OnKeyDown(Key key)
    {
        if (!IsRunning) return;
        
        var direction = key switch
        {
            Key.Left => MovementDirection.Left,
            Key.Right => MovementDirection.Right,
            Key.Up => MovementDirection.Up,
            Key.Down => MovementDirection.Down,
            _ => MovementDirection.None
        };
        
        if (direction != MovementDirection.None)
            _game.HandleInput(direction);
    }

    private void OnTick(object? sender, ElapsedEventArgs e)
    {
        try
        {
            _game.Update();
            
            Dispatcher.UIThread.Post(() =>
            {
                Score = _game.State.Score;
                PlayerX = (int)_game.Player.X;
                PlayerY = (int)_game.Player.Y;
                PlayerSprite = GetPlayerSprite(_game.Player.CurrentDirection);
                UpdateGhostData();
            });
        }
        catch { }
    }

    private void UpdateGhostData()
    {
        GhostData.Clear();
        foreach (var g in _game.Ghosts)
            GhostData.Add(new GhostData(new Position((int)g.X, (int)g.Y), GetGhostSprite(g.Type)));
    }

    private static string GetPlayerSprite(MovementDirection dir) => dir switch
    {
        MovementDirection.Up => "Assets/PacMan_Up.png",
        MovementDirection.Down => "Assets/PacMan_Down.png",
        MovementDirection.Left => "Assets/PacMan_Left.png",
        MovementDirection.Right => "Assets/PacMan_Right.png",
        _ => "Assets/PacMan_Default.png"
    };

    private static string GetGhostSprite(GhostType type) => type switch
    {
        GhostType.Blinky => "Assets/Ghost_Red.png",
        GhostType.Pinky => "Assets/Ghost_Pink.png",
        GhostType.Inky => "Assets/Ghost_Blue.png",
        GhostType.Clyde => "Assets/Ghost_Orange.png",
        _ => "Assets/Ghost_Default.png"
    };

    [RelayCommand]
    public void AddScoreAndStop()
    {
        _scoreBoard.AddScore(Score);
        OnPropertyChanged(nameof(TopScores));
        StopGame();
    }
}
