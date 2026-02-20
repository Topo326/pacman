using System;
using System.Collections.Generic;
using System.Linq;
using PacmanGame.Models;

namespace PacmanGame.Services;

public interface IGameService
{
    void Initialize();
    void Update();
    void HandleInput(MovementDirection direction);
    
    GameState State { get; }
    Player Player { get; }
    IReadOnlyList<Ghost> Ghosts { get; }
    GameMap Map { get; }
    IReadOnlyList<(double x, double y)> Dots { get; }
}

public class GameService : IGameService
{
    private readonly List<Ghost> _ghosts = new();
    private readonly List<(double x, double y)> _dots = new();
    private readonly ISoundService _soundService;
    
    public GameState State { get; } = new();
    public Player Player { get; private set; } = null!;
    public IReadOnlyList<Ghost> Ghosts => _ghosts;
    public GameMap Map { get; private set; } = null!;
    public IReadOnlyList<(double x, double y)> Dots => _dots;

    public GameService()
    {
        _soundService = new LinuxSoundService();
    }

    public void Initialize()
    {
        State.Reset();
        LoadHighScore(); // Load high score
        Map = new GameMap(TileMap.Map);
        InitializePlayer();
        InitializeGhosts();
        InitializeDots();
        _soundService.PlayBeginning();
    }

    private void InitializePlayer()
    {
        Player = new Player(6 * GameMap.TileSize, 22 * GameMap.TileSize);
    }

    private void InitializeGhosts()
    {
        _ghosts.Clear();
        int ts = GameMap.TileSize;
        
        // Speed 2.5 is default in Ghost class now
        _ghosts.Add(new Ghost(6 * ts, 5 * ts, GhostType.Blinky, 0, true));
        _ghosts.Add(new Ghost(12 * ts, 13 * ts, GhostType.Pinky, 10));
        _ghosts.Add(new Ghost(15 * ts, 13 * ts, GhostType.Inky, 20));
        _ghosts.Add(new Ghost(13 * ts, 14 * ts, GhostType.Clyde, 30));
    }

    private void InitializeDots()
    {
        _dots.Clear();
        int ts = GameMap.TileSize;
        
        for (int r = 0; r < Map.Rows; r++)
        for (int c = 0; c < Map.Cols; c++)
        {
            if (Map[r, c] == 0)
                _dots.Add((c * ts + 8, r * ts + 8));
        }
    }

    public void HandleInput(MovementDirection direction)
    {
        Player.RequestedDirection = direction;
    }

    public void Update()
    {
        State.ElapsedSeconds += 0.016;
        
        ActivateGhosts();
        Player.Update(Map);
        UpdateGhosts();
        CheckDotCollision();
    }

    private void ActivateGhosts()
    {
        foreach (var ghost in _ghosts.Where(g => !g.IsActive && State.ElapsedSeconds >= g.ReleaseTime))
            ghost.IsActive = true;
    }

    private void UpdateGhosts()
    {
        var blinky = _ghosts.FirstOrDefault(g => g.Type == GhostType.Blinky);
        foreach (var ghost in _ghosts.Where(g => g.IsActive))
        {
            ghost.Update(Map, Player, blinky);
            
            // Check Collision with Player
            double dist = Math.Sqrt(Math.Pow(ghost.X - Player.X, 2) + Math.Pow(ghost.Y - Player.Y, 2));
            if (dist < 15)
            {
                HandleDeath();
            }
        }
    }

    private void HandleDeath()
    {
        _soundService.PlayDeath();
        State.Lives--;
        
        if (State.Lives > 0)
        {
            // Reset positions
            InitializePlayer();
            InitializeGhosts();
        }
        else
        {
    
            SaveHighScore();
            InitializePlayer();
            InitializeGhosts();
            State.Reset();
            LoadHighScore();
        }
    }

    private void CheckDotCollision()
    {
        const double collisionRadius = 14;
        
        for (int i = _dots.Count - 1; i >= 0; i--)
        {
            var dot = _dots[i];
            double dx = Math.Abs(dot.x + 4 - Player.CenterX);
            double dy = Math.Abs(dot.y + 4 - Player.CenterY);
            
            if (dx < collisionRadius && dy < collisionRadius)
            {
                _dots.RemoveAt(i);
                State.AddScore(10);
                _soundService.PlayChomp();
            }
        }
    }

    private void LoadHighScore()
    {
        try
        {
            string path = "highscore.json";
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                var data = System.Text.Json.JsonSerializer.Deserialize<HighScoreData>(json);
                if (data != null)
                {
                    State.HighScore = data.Score;
                }
            }
        }
        catch { }
    }

    private void SaveHighScore()
    {
        try
        {
            if (State.Score > State.HighScore)
            {
                State.HighScore = State.Score;
            }
            
            var data = new HighScoreData { Score = State.HighScore };
            string json = System.Text.Json.JsonSerializer.Serialize(data);
            System.IO.File.WriteAllText("highscore.json", json);
        }
        catch { }
    }

    private class HighScoreData
    {
        public int Score { get; set; }
    }
}
