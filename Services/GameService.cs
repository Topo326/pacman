using System;
using System.Collections.Generic;
using System.Linq;
using PacmanGame.Models;

namespace PacmanGame.Services;

/// <summary>
/// Define los contratos públicos para el ciclo de vida, eventos y acceso a datos del núcleo del juego.
/// </summary>
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
    Pickup? ActivePickup { get; }
}

/// <summary>
/// Servicio principal delegado encargado de orquestar la lógica del juego de Pac-Man.
/// Se encarga de instanciar entidades, gestionar colisiones, controlar la línea temporal de la partida y actualizar puntuaciones.
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
    public Pickup? ActivePickup { get; private set; }

    /// <summary>
    /// Constructor del servicio inicializador de instanciamientos y manejador de eventos del estado del juego.
    /// </summary>
    public GameService()
    {
        _soundService = new LinuxSoundService();
        _scoreBoard = new ScoreBoard();
        State.OnModeChanged += HandleModeChanged;
    }
    
    /// <summary>
    /// Evento disparador automático (clásico del juego arcade) que invierte instantáneamente la dirección de todos los fantasmas 
    /// en el momento que alternan entre los modos Chase, Scatter y Frightened.
    /// </summary>
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

    /// <summary>
    /// Inicializa y construye un nuevo mapa de juego, preparando jugadores, fantasmas, puntos, e invocando la música inicial.
    /// También recupera la mejor puntuación histórica registrada antes de comenzar.
    /// </summary>
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
        ActivePickup = null;
        _soundService.PlayBeginning();
    }

    /// <summary>
    /// Instancia la clase de Pac-Man asignándole su posición de inicio estática original.
    /// </summary>
    private void InitializePlayer()
    {
        Player = new Player(13 * GameConstants.TileSize, 22 * GameConstants.TileSize);
    }

    /// <summary>
    /// Instancia a los 4 fantasmas clásicos, otorgando diferentes instantes de lanzamiento y encasillándolos dentro o fuera de la casa.
    /// </summary>
    private void InitializeGhosts()
    {
        _ghosts.Clear();
        int ts = GameConstants.TileSize;
        
        _ghosts.Add(new Ghost(13 * ts, 11 * ts, GhostType.Blinky, 0, true));
        _ghosts.Add(new Ghost(13 * ts, 14 * ts, GhostType.Pinky, 5));
        _ghosts.Add(new Ghost(11.5 * ts, 14 * ts, GhostType.Inky, 10));
        _ghosts.Add(new Ghost(14.5 * ts, 14 * ts, GhostType.Clyde, 15));
    }

    /// <summary>
    /// Escanea las baldosas permitidas en la matriz inicial y posiciona dinámicamente cada píldora y Píldora de Poder interactiva en el mapa.
    /// </summary>
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

    /// <summary>
    /// Recibe un comando externo (por ejemplo, del teclado en un ViewModel) que indica la intención del jugador de moverse a una dirección específica.
    /// </summary>
    public void HandleInput(MovementDirection direction)
    {
        Player.RequestedDirection = direction;
    }

    /// <summary>
    /// Ciclo principal del juego, típicamente llamado cada 16ms (un tick a ~60 FPS).
    /// Coordina la actualización de las físicas, colisiones, render, recolección de puntos y las muertes controlando su animación.
    /// </summary>
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
        UpdatePickups();
        CheckDotCollision();
        
        if (State.IsGameOver)
        {
            HandleGameOver();
        }
    }

    /// <summary>
    /// Gestiona la duración y aparición (spawn) de las frutas (Cereza y Fresa).
    /// </summary>
    private void UpdatePickups()
    {
        if (ActivePickup != null)
        {
            ActivePickup.Update(0.016);
            if (!ActivePickup.IsActive)
            {
                DespawnActivePickup();
            }
        }
        else
        {
            if (State.CherrySpawnTimer <= 0)
            {
                var (x, y) = GetRandomWalkablePosition();
                ActivePickup = new Pickup(x, y, PickupType.Cherry, 12.0);
                State.CherrySpawnTimer = double.MaxValue; // Prevenir múltiples spawns
            }
            else if (State.StrawberrySpawnTimer <= 0)
            {
                var (x, y) = GetRandomWalkablePosition();
                ActivePickup = new Pickup(x, y, PickupType.Strawberry, 12.0);
                State.StrawberrySpawnTimer = double.MaxValue;
            }
        }
    }

    /// <summary>
    /// Escanéa las baldosas transitables (caminos normales) y selecciona una de manera aleatoria segura.
    /// </summary>
    private (double x, double y) GetRandomWalkablePosition()
    {
        var random = new Random();
        var validTiles = new List<(int r, int c)>();
        for (int r = 0; r < Map.Rows; r++)
        {
            for (int c = 0; c < Map.Cols; c++)
            {
                if (Map[r, c] == 0) // Pasillos donde pueden haber puntos
                {
                    validTiles.Add((r, c));
                }
            }
        }
        
        if (validTiles.Any())
        {
            var selected = validTiles[random.Next(validTiles.Count)];
            return (selected.c * GameConstants.TileSize, selected.r * GameConstants.TileSize);
        }
        
        // Punto de aparición por defecto (debajo de la casa de fantasmas) si falla el escaneo
        return (13 * GameConstants.TileSize, 22 * GameConstants.TileSize);
    }

    /// <summary>
    /// Limpia el pickup activo y reinicia su temporizador correspondiente para que vuelva a aparecer en el futuro.
    /// </summary>
    private void DespawnActivePickup()
    {
        if (ActivePickup == null) return;
        
        if (ActivePickup.Type == PickupType.Cherry)
            State.CherrySpawnTimer = 15.0;
        else if (ActivePickup.Type == PickupType.Strawberry)
            State.StrawberrySpawnTimer = 30.0;
            
        ActivePickup = null;
    }

    /// <summary>
    /// Revisa si el timer personal del fantasma encerrado ha caducado, otorgándole permiso para salir de la casa.
    /// </summary>
    private void ActivateGhosts()
    {
        foreach (var ghost in _ghosts.Where(g => !g.IsActive && State.ElapsedSeconds >= g.ReleaseTime))
            ghost.IsActive = true;
    }

    /// <summary>
    /// Ejecuta la IA perimetral de cada fantasma e inspecciona si chocan geográficamente contra Pac-Man o si son comidos.
    /// </summary>
    private void UpdateGhosts()
    {
        var blinky = _ghosts.FirstOrDefault(g => g.Type == GhostType.Blinky);
        foreach (var ghost in _ghosts.Where(g => g.IsActive))
        {
            ghost.Update(Map, Player, blinky, State.IsFrightenedMode, State.IsScatterMode);
            
            // Comprobar colisión letal con el jugador basándose en tolerancia de distancia inter-centro
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

    /// <summary>
    /// Lógica ejecutada al capturar un fantasma vulnerable. Le suma puntos al jugador y envía al fantasma inactivo temporalmente o de vuelta a la casa.
    /// </summary>
    private void EatGhost(Ghost ghost)
    {
        ghost.IsEaten = true;
        State.AddScore(GameConstants.GhostEatScore);
        _soundService.PlayEatGhost();
        
        // En un Pac-Man real, el fantasma vuelve volando a la casa. Aquí lo marcamos como comido.
        // Lo reseteamos a las coordenadas centrales secretas en la casa de fantasmas:
        ghost.Reset(13 * GameConstants.TileSize, 14 * GameConstants.TileSize);
        ghost.IsActive = false;
        ghost.ReleaseTime = State.ElapsedSeconds + 5; // Configurado internamente para reaparecer en 5 segundos
    }

    /// <summary>
    /// Dispara inmediatamente un registro de "Pérdida de Vida", activa los audios de muerte y congela a las demás iteraciones usando retraso (timer).
    /// </summary>
    private void HandleDeath()
    {
        if (Player.IsDead) return;
        _soundService.PlayDeath();
        Player.IsDead = true;
        State.DeathTimer = 1.6; // Tiempo de descanso intencional para ver o sentir la animación de muerte
    }

    /// <summary>
    /// Procede al estado final y perdedor para archivar la puntuación (Top Score). 
    /// El reinicio ahora es delegado manualmente al UI.
    /// </summary>
    private void HandleGameOver()
    {
        _scoreBoard.AddScore(State.Score);
    }

    /// <summary>
    /// Filtra y elimina aquellos puntos (Dot) del entorno recolectados por Pac-Man mediante un escáner de intersección de dimensiones del hitbox.
    /// Alterna el modo vulnerabilidad si se come una Píldora de Poder y restablece la partida como 'Victoria' si eliminaron todos los puntos.
    /// </summary>
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
                    
                    // Al comer otra Píldora de Poder nueva, se asume reiniciar la contaduría y estado vulnerable de encierro si estaban antes
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

        // Revisamos colisión con la Fruta (Pickup) activa
        if (ActivePickup != null && ActivePickup.IsActive)
        {
            double pdx = Math.Abs((ActivePickup.X + GameConstants.TileSize / 2.0) - Player.CenterX);
            double pdy = Math.Abs((ActivePickup.Y + GameConstants.TileSize / 2.0) - Player.CenterY);
            
            if (pdx < collisionRadius && pdy < collisionRadius)
            {
                if (ActivePickup.Type == PickupType.Cherry)
                {
                    State.AddScore(1000);
                }
                else if (ActivePickup.Type == PickupType.Strawberry)
                {
                    if (State.Lives < 3)
                    {
                        State.Lives++;
                        State.AddScore(1000);
                    }
                    else
                    {
                        State.AddScore(2500);
                    }
                }
                _soundService.PlayEatFruit();
                DespawnActivePickup(); // Destruir tras consumo y reiniciar timer
            }
        }

        if (!_dots.Any())
        {
            // Nivel completado (Limpió todos los puntos)
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
