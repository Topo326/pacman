using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly SpriteService _spriteService = SpriteService.Instance;
    private Timer _tick;
    private TileMap _map;
    private PlayerEntity _player;
    private readonly List<GhostEntity> _ghosts = new();
    private readonly ScoreBoard _scoreBoard = new();
    private Process? _audioProcess;

    private readonly int[,] _initialMap = new int[,]
    {
        {1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,1,0,1,1,1,1,0,1},
        {1,0,1,0,0,0,0,0,1,0,1},
        {1,0,1,0,1,1,1,0,1,0,1},
        {1,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1}
    };

    [ObservableProperty]
    private int playerX;

    [ObservableProperty]
    private int playerY;

    [ObservableProperty]
    private int score;

    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private string playerSprite;

    public ObservableCollection<Position> GhostPositions { get; } = new();
    public ObservableCollection<GhostData> GhostData { get; } = new();
    public IReadOnlyList<int> TopScores => _scoreBoard.TopScores;
    public ObservableCollection<Tile> Map { get; } = new();

    public SpriteService SpriteService => _spriteService;

    public GameViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _main = mainWindowViewModel;
        PlayerSprite = _spriteService.GetPlayerSprite("default");
        _tick = new Timer(120);
        _map = new TileMap(_initialMap);
        _player = new PlayerEntity(new Position(0, 0));
    }

    [RelayCommand]
    public void StartGame()
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

        _player = new PlayerEntity(new Position(5, 5));
        _player.Dx = 0; _player.Dy = -1;

        _ghosts.Clear();
        _ghosts.Add(new GhostEntity(new Position(5, 1), GhostType.Blinky));
        _ghosts.Add(new GhostEntity(new Position(4, 1), GhostType.Pinky));
        _ghosts.Add(new GhostEntity(new Position(6, 1), GhostType.Inky));
        _ghosts.Add(new GhostEntity(new Position(1, 5), GhostType.Clyde));

        GhostData.Clear();
        foreach (var g in _ghosts)
        {
            GhostData.Add(new GhostData(g.Pos, _spriteService.GetGhostSprite(g.Type)));
        }

        Score = 0;
        PlayerX = _player.Pos.X; PlayerY = _player.Pos.Y;

        PlayerSprite = _spriteService.GetPlayerSprite("default");

        _tick.Elapsed -= OnTick;
        _tick.Elapsed += OnTick;
        _tick.AutoReset = true;
        IsRunning = true;
        _tick.Start();
    }

    [RelayCommand]
    public void StopGame()
    {
        _tick?.Stop();
        IsRunning = false;
    }

    [RelayCommand]
    public void OnKeyDown(Key key)
    {
        if (!IsRunning || _player is null) return;
        switch (key)
        {
            case Key.Left:
                _player.DesiredDx = -1; _player.DesiredDy = 0; break;
            case Key.Right:
                _player.DesiredDx = 1; _player.DesiredDy = 0; break;
            case Key.Up:
                _player.DesiredDx = 0; _player.DesiredDy = -1; break;
            case Key.Down:
                _player.DesiredDx = 0; _player.DesiredDy = 1; break;
        }
    }

    private void OnTick(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var desiredNext = _player.Pos.Offset(_player.DesiredDx, _player.DesiredDy);
            if (_map.IsWalkable(desiredNext.X, desiredNext.Y))
            {
                _player.Dx = _player.DesiredDx; _player.Dy = _player.DesiredDy;
            }
            else
            {
                var currentNext = _player.Pos.Offset(_player.Dx, _player.Dy);
                if (!_map.IsWalkable(currentNext.X, currentNext.Y))
                {
                    _player.Dx = 0; _player.Dy = 0;
                }
            }
            var next = _player.NextTile();
            if (_map.IsWalkable(next.X, next.Y)) _player.Step();

            foreach (var g in _ghosts)
            {
                var blinky = _ghosts.Find(x => x.Type == GhostType.Blinky) ?? g;
                var target = g.ComputeTarget(_player.Pos, _player.Dx, _player.Dy, blinky.Pos, _map);
                g.ChooseDirectionTowards(target, _map);
                var gnext = g.NextTile();
                if (_map.IsWalkable(gnext.X, gnext.Y)) g.Step();
            }

            var collided = _ghosts.Exists(g => g.Pos.X == _player.Pos.X && g.Pos.Y == _player.Pos.Y);
            if (collided)
            {
                Score = Math.Max(0, Score - 100);
                _player.Pos = new Position(5, 5);
            }

            PlayerSprite = _spriteService.GetPlayerSprite(_player.Dx, _player.Dy);

            Dispatcher.UIThread.Post(() =>
            {
                GhostData.Clear();
                foreach (var g in _ghosts)
                {
                    GhostData.Add(new GhostData(g.Pos, _spriteService.GetGhostSprite(g.Type)));
                }

                PlayerX = _player.Pos.X; PlayerY = _player.Pos.Y;
                GhostPositions.Clear();
                foreach (var g in _ghosts) GhostPositions.Add(g.Pos);
                OnPropertyChanged(nameof(TopScores));
            });
        }
        catch { }
    }

    [RelayCommand]
    public void AddScoreAndStop()
    {
        _scoreBoard.AddScore(Score);
        OnPropertyChanged(nameof(TopScores));
        StopGame();
    }
}
