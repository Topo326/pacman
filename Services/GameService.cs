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
    IReadOnlyList<(double x, double y, bool isPower)> Dots { get; }
}

/// <summary>
/// Servicio principal que orquestra la lógica del juego Pacman.
/// </summary>
public class GameService : IGameService
{
    private readonly List<Ghost> _ghosts = new();
    private readonly List<(double x, double y, bool isPower)> _dots = new();
    private readonly ISoundService _soundService;
    private readonly ScoreBoard _scoreBoard;
    
    public GameState State { get; } = new();
    public Player Player { get; private set; } = null!;
    public IReadOnlyList<Ghost> Ghosts => _ghosts;
    public GameMap Map { get; private set; } = null!;
    public IReadOnlyList<(double x, double y, bool isPower)> Dots => _dots;

    public GameService()
    {
        _soundService = new LinuxSoundService();
        _scoreBoard = new ScoreBoard();
        State.OnModeChanged += HandleModeChanged;
    }
    
    private void HandleModeChanged()
    {
        foreach (var ghost in _ghosts.Where(g => g.IsActive && !g.IsEaten))
        {
            var opposite = ghost.CurrentDirection.Opposite();
            if (opposite != MovementDirection.None)
            {
                ghost.CurrentDirection = opposite;
            }
        }
    }

    public void Initialize()
    {
        State.Reset();
        // Cargar el récord más alto de la ScoreBoard
        if (_scoreBoard.TopScores.Any())
        {
            State.HighScore = _scoreBoard.TopScores.Max();
        }

        Map = new GameMap(TileMap.Map);
        InitializePlayer();
        InitializeGhosts();
        InitializeDots();
        _soundService.PlayBeginning();
    }

    private void InitializePlayer()
    {
        Player = new Player(13 * GameConstants.TileSize, 22 * GameConstants.TileSize);
    }

    private void InitializeGhosts()
    {
        _ghosts.Clear();
        int ts = GameConstants.TileSize;
        
        _ghosts.Add(new Ghost(13 * ts, 11 * ts, GhostType.Blinky, 0, true));
        _ghosts.Add(new Ghost(13 * ts, 14 * ts, GhostType.Pinky, 5));
        _ghosts.Add(new Ghost(11.5 * ts, 14 * ts, GhostType.Inky, 10));
        _ghosts.Add(new Ghost(14.5 * ts, 14 * ts, GhostType.Clyde, 15));
    }

    private void InitializeDots()
    {
        _dots.Clear();
        int ts = GameConstants.TileSize;
        
        for (int r = 0; r < Map.Rows; r++)
        for (int c = 0; c < Map.Cols; c++)
        {
            if (Map[r, c] == 0) // Punto normal
                _dots.Add((c * ts + 8, r * ts + 8, false));
            else if (Map[r, c] == 3) // Súper Píldora
                _dots.Add((c * ts + 4, r * ts + 4, true));
        }
    }

    public void HandleInput(MovementDirection direction)
    {
        Player.RequestedDirection = direction;
    }

    public void Update()
    {
        State.Update(0.016); // Aproximadamente 60 FPS
        
        if (Player.IsDead)
        {
            State.DeathTimer -= 0.016;
            if (State.DeathTimer <= 0)
            {
                State.LoseLife();
                if (State.Lives > 0)
                {
                    Player.Reset(13 * GameConstants.TileSize, 22 * GameConstants.TileSize);
                    State.IsFrightenedMode = false;
                    State.FrightenedTimeLeft = 0;
                    State.IsScatterMode = false;
                    State.ModeTimeLeft = GameConstants.ChaseDurationSeconds;
                    InitializeGhosts();
                }
                else
                {
                    HandleGameOver();
                }
            }
            return;
        }
        
        ActivateGhosts();
        Player.Update(Map);
        UpdateGhosts();
        CheckDotCollision();
        
        if (State.IsGameOver)
        {
            HandleGameOver();
        }
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
            ghost.Update(Map, Player, blinky, State.IsFrightenedMode, State.IsScatterMode);
            
            // Comprobar colisión con el jugador
            double dist = Math.Sqrt(Math.Pow(ghost.CenterX - Player.CenterX, 2) + Math.Pow(ghost.CenterY - Player.CenterY, 2));
            if (dist < 20 && !Player.IsDead)
            {
                if (State.IsFrightenedMode && !ghost.IsEaten)
                {
                    EatGhost(ghost);
                }
                else if (!ghost.IsEaten)
                {
                    HandleDeath();
                }
            }
        }
    }

    private void EatGhost(Ghost ghost)
    {
        ghost.IsEaten = true;
        State.AddScore(GameConstants.GhostEatScore);
        _soundService.PlayEatGhost();
        // En un Pacman real, el fantasma vuelve a la casa. Aquí lo marcamos como comido.
        // Podríamos resetearlo a la casa de fantasmas:
        ghost.Reset(13 * GameConstants.TileSize, 14 * GameConstants.TileSize);
        ghost.IsActive = false;
        ghost.ReleaseTime = State.ElapsedSeconds + 5; // Reaparece en 5 segundos
    }

    private void HandleDeath()
    {
        if (Player.IsDead) return;
        _soundService.PlayDeath();
        Player.IsDead = true;
        State.DeathTimer = 1.6; // tiempo para animación de muerte
    }

    private void HandleGameOver()
    {
        _scoreBoard.AddScore(State.Score);
        Initialize(); // Reiniciar el juego
    }

    private void CheckDotCollision()
    {
        const double collisionRadius = 15;
        
        for (int i = _dots.Count - 1; i >= 0; i--)
        {
            var dot = _dots[i];
            double dotCenterX = dot.isPower ? dot.x + 8 : dot.x + 4;
            double dotCenterY = dot.isPower ? dot.y + 8 : dot.y + 4;
            
            double dx = Math.Abs(dotCenterX - Player.CenterX);
            double dy = Math.Abs(dotCenterY - Player.CenterY);
            
            if (dx < collisionRadius && dy < collisionRadius)
            {
                if (dot.isPower)
                {
                    State.AddScore(GameConstants.PowerPillScore);
                    State.ActivateFrightenedMode();
                    // Resetear fantasmas comidos al activar nueva píldora
                    foreach(var g in _ghosts) g.IsEaten = false;
                }
                else
                {
                    State.AddScore(GameConstants.DotScore);
                }
                
                _dots.RemoveAt(i);
                _soundService.PlayChomp();
            }
        }

        if (!_dots.Any())
        {
            // Nivel completado
            InitializeDots();
            Player.Reset(13 * GameConstants.TileSize, 22 * GameConstants.TileSize);
            State.IsFrightenedMode = false;
            State.FrightenedTimeLeft = 0;
            State.IsScatterMode = false;
            State.ModeTimeLeft = GameConstants.ChaseDurationSeconds;
            InitializeGhosts();
        }
    }
}
