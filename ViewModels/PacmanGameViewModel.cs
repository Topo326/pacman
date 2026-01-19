using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Input;
using Avalonia.Threading;
using PacmanGame.Models;

namespace PacmanGame.ViewModels;

public class MapTile
{
    public double X { get; set; }
    public double Y { get; set; }
    public int Type { get; set; }
}

public class SimpleGhost
{
    public double X { get; set; }
    public double Y { get; set; }
    public string Color { get; set; } = "Red";
}

public partial class PacmanGameViewModel : ViewModelBase
{
    private const int BLOCK_SIZE = 20;
    private DispatcherTimer? _gameTimer;
    private DispatcherTimer? _animationTimer;
    private Process? _audioProcess;
    private readonly Random _random = new();
    
    private readonly List<SimpleGhost> _ghosts = new();
    
    private int[,] _gameMap = new int[,]
    {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,2,2,2,2,2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,1},
        {1,2,1,1,1,2,1,1,1,2,1,2,1,1,1,2,1,1,1,2,1},
        {1,2,1,1,1,2,1,1,1,2,1,2,1,1,1,2,1,1,1,2,1},
        {1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1},
        {1,2,1,1,1,2,1,2,1,1,1,1,1,2,1,2,1,1,1,2,1},
        {1,2,2,2,2,2,1,2,2,2,1,2,2,2,1,2,2,2,2,2,1},
        {1,1,1,1,1,2,1,1,1,2,1,2,1,1,1,2,1,1,1,1,1},
        {0,0,0,0,1,2,1,2,2,2,2,2,2,2,1,2,1,0,0,0,0},
        {1,1,1,1,1,2,1,2,1,1,2,1,1,2,1,2,1,1,1,1,1},
        {2,2,2,2,2,2,2,2,1,2,2,2,1,2,2,2,2,2,2,2,2},
        {1,1,1,1,1,2,1,2,1,2,2,2,1,2,1,2,1,1,1,1,1},
        {0,0,0,0,1,2,1,2,1,1,1,1,1,2,1,2,1,0,0,0,0},
        {0,0,0,0,1,2,1,2,2,2,2,2,2,2,1,2,1,0,0,0,0},
        {1,1,1,1,1,2,2,2,1,1,1,1,1,2,2,2,1,1,1,1,1},
        {1,2,2,2,2,2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,1},
        {1,2,1,1,1,2,1,1,1,2,1,2,1,1,1,2,1,1,1,2,1},
        {1,2,2,2,1,2,2,2,2,2,1,2,2,2,2,2,1,2,2,2,1},
        {1,1,2,2,1,2,1,2,1,1,1,1,1,2,1,2,1,2,2,1,1},
        {1,2,2,2,2,2,1,2,2,2,1,2,2,2,1,2,2,2,2,2,1},
        {1,2,1,1,1,1,1,1,1,2,1,2,1,1,1,1,1,1,1,2,1},
        {1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
    };
    
    [ObservableProperty] private PacmanViewModel? pacman;
    [ObservableProperty] private int score;
    [ObservableProperty] private int lives = 3;
    [ObservableProperty] private bool isGameRunning;
    [ObservableProperty] private int canvasWidth = 420;
    [ObservableProperty] private int canvasHeight = 460;
    
    public ObservableCollection<MapTile> MapTiles { get; } = new();
    public ObservableCollection<SimpleGhost> Ghosts { get; } = new();
    
    public string PacmanSprite
    {
        get
        {
            if (Pacman == null) return "avares://PacmanGame/Assets/Arcade - Pac-Man - Miscellaneous - General Sprites.png";
            
            return Pacman.Direction switch
            {
                Direction.Right => "avares://PacmanGame/Assets/Arcade - Pac-Man - General Sprites - Blinky (RIght).gif",
                Direction.Left => "avares://PacmanGame/Assets/Arcade - Pac-Man - General Sprites - Blinky (Left).gif",
                Direction.Up => "avares://PacmanGame/Assets/Arcade - Pac-Man - General Sprites - Blinky (Up).gif",
                Direction.Bottom => "avares://PacmanGame/Assets/Arcade - Pac-Man - General Sprites - Blinky (Down).gif",
                _ => "avares://PacmanGame/Assets/Arcade - Pac-Man - Miscellaneous - General Sprites.png"
            };
        }
    }
    
    public PacmanGameViewModel()
    {
        InitializeGame();
        RenderMap();
    }
    
    private void InitializeGame()
    {
        Pacman = new PacmanViewModel(
            x: BLOCK_SIZE,
            y: BLOCK_SIZE,
            width: BLOCK_SIZE,
            height: BLOCK_SIZE,
            speed: BLOCK_SIZE / 5.0
        );
    }
    
    private void RenderMap()
    {
        MapTiles.Clear();
        for (int i = 0; i < _gameMap.GetLength(0); i++)
        {
            for (int j = 0; j < _gameMap.GetLength(1); j++)
            {
                MapTiles.Add(new MapTile
                {
                    X = j * BLOCK_SIZE,
                    Y = i * BLOCK_SIZE,
                    Type = _gameMap[i, j]
                });
            }
        }
    }
    
    private void InitializeGhosts()
    {
        _ghosts.Clear();
        Ghosts.Clear();
        
        var ghostColors = new[] { "Red", "Pink", "Cyan", "Orange" };
        var ghostPositions = new[] 
        { 
            (9*BLOCK_SIZE, 9*BLOCK_SIZE), 
            (10*BLOCK_SIZE, 9*BLOCK_SIZE),
            (11*BLOCK_SIZE, 9*BLOCK_SIZE),
            (9*BLOCK_SIZE, 10*BLOCK_SIZE)
        };
        
        for (int i = 0; i < 4; i++)
        {
            var ghost = new SimpleGhost
            {
                X = ghostPositions[i].Item1,
                Y = ghostPositions[i].Item2,
                Color = ghostColors[i]
            };
            _ghosts.Add(ghost);
            Ghosts.Add(ghost);
        }
    }
    
    [RelayCommand]
    public void StartGame()
    {
        if (IsGameRunning) return;
        
        Score = 0;
        Lives = 3;
        
        _gameMap = new int[,]
        {
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,2,2,2,2,2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,1},
            {1,2,1,1,1,2,1,1,1,2,1,2,1,1,1,2,1,1,1,2,1},
            {1,2,1,1,1,2,1,1,1,2,1,2,1,1,1,2,1,1,1,2,1},
            {1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1},
            {1,2,1,1,1,2,1,2,1,1,1,1,1,2,1,2,1,1,1,2,1},
            {1,2,2,2,2,2,1,2,2,2,1,2,2,2,1,2,2,2,2,2,1},
            {1,1,1,1,1,2,1,1,1,2,1,2,1,1,1,2,1,1,1,1,1},
            {0,0,0,0,1,2,1,2,2,2,2,2,2,2,1,2,1,0,0,0,0},
            {1,1,1,1,1,2,1,2,1,1,2,1,1,2,1,2,1,1,1,1,1},
            {2,2,2,2,2,2,2,2,1,2,2,2,1,2,2,2,2,2,2,2,2},
            {1,1,1,1,1,2,1,2,1,2,2,2,1,2,1,2,1,1,1,1,1},
            {0,0,0,0,1,2,1,2,1,1,1,1,1,2,1,2,1,0,0,0,0},
            {0,0,0,0,1,2,1,2,2,2,2,2,2,2,1,2,1,0,0,0,0},
            {1,1,1,1,1,2,2,2,1,1,1,1,1,2,2,2,1,1,1,1,1},
            {1,2,2,2,2,2,2,2,2,2,1,2,2,2,2,2,2,2,2,2,1},
            {1,2,1,1,1,2,1,1,1,2,1,2,1,1,1,2,1,1,1,2,1},
            {1,2,2,2,1,2,2,2,2,2,1,2,2,2,2,2,1,2,2,2,1},
            {1,1,2,2,1,2,1,2,1,1,1,1,1,2,1,2,1,2,2,1,1},
            {1,2,2,2,2,2,1,2,2,2,1,2,2,2,1,2,2,2,2,2,1},
            {1,2,1,1,1,1,1,1,1,2,1,2,1,1,1,1,1,1,1,2,1},
            {1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
        };
        
        RenderMap();
        InitializeGhosts();
        
        Pacman = new PacmanViewModel(
            x: BLOCK_SIZE,
            y: BLOCK_SIZE,
            width: BLOCK_SIZE,
            height: BLOCK_SIZE,
            speed: 4
        );
        
        PlaySound("Assets/pacman-beginning./pacman_beginning.wav");
        
        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / 30)
        };
        _gameTimer.Tick += OnGameTick;
        _gameTimer.Start();
        
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _animationTimer.Tick += (s, e) => Pacman?.ChangeAnimation();
        _animationTimer.Start();
        
        IsGameRunning = true;
    }
    
    [RelayCommand]
    public void StopGame()
    {
        _gameTimer?.Stop();
        _animationTimer?.Stop();
        _audioProcess?.Kill();
        IsGameRunning = false;
    }
    
    private void PlaySound(string audioFile)
    {
        try
        {
            _audioProcess?.Kill();
            _audioProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "aplay",
                    Arguments = audioFile,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            _audioProcess.Start();
        }
        catch { }
    }
    
    private void OnGameTick(object? sender, EventArgs e)
    {
        if (Pacman == null) return;
        
        Pacman.MoveProcess(_gameMap);
        MoveGhosts();
        CheckFoodCollision();
        CheckGhostCollision();
        CheckVictory();
        OnPropertyChanged(nameof(PacmanSprite));
    }
    
    private void MoveGhosts()
    {
        if (Pacman == null) return;
        
        foreach (var ghost in _ghosts)
        {
            int ghostMapX = (int)(ghost.X / BLOCK_SIZE);
            int ghostMapY = (int)(ghost.Y / BLOCK_SIZE);
            
            int pacmanMapX = Pacman.GetMapX();
            int pacmanMapY = Pacman.GetMapY();
            
            double moveSpeed = 2.0;
            
            if (_random.Next(0, 10) < 7)
            {
                if (pacmanMapX > ghostMapX && CanMove(ghost.X + moveSpeed, ghost.Y))
                    ghost.X += moveSpeed;
                else if (pacmanMapX < ghostMapX && CanMove(ghost.X - moveSpeed, ghost.Y))
                    ghost.X -= moveSpeed;
                
                if (pacmanMapY > ghostMapY && CanMove(ghost.X, ghost.Y + moveSpeed))
                    ghost.Y += moveSpeed;
                else if (pacmanMapY < ghostMapY && CanMove(ghost.X, ghost.Y - moveSpeed))
                    ghost.Y -= moveSpeed;
            }
            else
            {
                var directions = new[] { (moveSpeed, 0.0), (-moveSpeed, 0.0), (0.0, moveSpeed), (0.0, -moveSpeed) };
                var dir = directions[_random.Next(directions.Length)];
                if (CanMove(ghost.X + dir.Item1, ghost.Y + dir.Item2))
                {
                    ghost.X += dir.Item1;
                    ghost.Y += dir.Item2;
                }
            }
        }
        
        Ghosts.Clear();
        foreach (var g in _ghosts)
            Ghosts.Add(g);
    }
    
    private bool CanMove(double x, double y)
    {
        int row = (int)(y / BLOCK_SIZE);
        int col = (int)(x / BLOCK_SIZE);
        
        if (row < 0 || row >= _gameMap.GetLength(0) || col < 0 || col >= _gameMap.GetLength(1))
            return false;
        
        return _gameMap[row, col] != 1;
    }
    
    private void CheckGhostCollision()
    {
        if (Pacman == null) return;
        
        int pacX = Pacman.GetMapX();
        int pacY = Pacman.GetMapY();
        
        foreach (var ghost in _ghosts)
        {
            int gX = (int)(ghost.X / BLOCK_SIZE);
            int gY = (int)(ghost.Y / BLOCK_SIZE);
            
            if (Math.Abs(pacX - gX) <= 0 && Math.Abs(pacY - gY) <= 0)
            {
                Lives--;
                PlaySound("Assets/pacman-death./pacman_death.wav");
                
                if (Lives <= 0)
                {
                    GameOver();
                }
                else
                {
                    Pacman.X = BLOCK_SIZE;
                    Pacman.Y = BLOCK_SIZE;
                }
                break;
            }
        }
    }
    
    private void CheckFoodCollision()
    {
        if (Pacman == null) return;
        
        int mapX = Pacman.GetMapX();
        int mapY = Pacman.GetMapY();
        
        if (mapY >= 0 && mapY < _gameMap.GetLength(0) && 
            mapX >= 0 && mapX < _gameMap.GetLength(1))
        {
            if (_gameMap[mapY, mapX] == 2)
            {
                _gameMap[mapY, mapX] = 3;
                Score += 10;
                PlaySound("Assets/pacman-chomp./pacman_chomp.wav");
                RenderMap();
            }
        }
    }
    
    private void CheckVictory()
    {
        bool hasFood = false;
        for (int i = 0; i < _gameMap.GetLength(0); i++)
        {
            for (int j = 0; j < _gameMap.GetLength(1); j++)
            {
                if (_gameMap[i, j] == 2)
                {
                    hasFood = true;
                    break;
                }
            }
            if (hasFood) break;
        }
        
        if (!hasFood)
        {
            StopGame();
            PlaySound("Assets/pacman-intermission./pacman_intermission.wav");
        }
    }
    
    private void GameOver()
    {
        StopGame();
        Lives = 0;
    }
    
    [RelayCommand]
    public void OnKeyDown(KeyEventArgs e)
    {
        if (!IsGameRunning || Pacman == null) return;
        
        switch (e.Key)
        {
            case Key.Left or Key.A:
                Pacman.NextDirection = Direction.Left;
                break;
            case Key.Right or Key.D:
                Pacman.NextDirection = Direction.Right;
                break;
            case Key.Up or Key.W:
                Pacman.NextDirection = Direction.Up;
                break;
            case Key.Down or Key.S:
                Pacman.NextDirection = Direction.Bottom;
                break;
        }
    }
}

