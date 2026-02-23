using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PacmanGame.Models;
using PacmanGame.Services;

namespace PacmanGame.ViewModels;

/// <summary>
/// Intermediario (ViewModel) que sincroniza en tiempo real los datos del motor del juego (Models)
/// con los componentes renderizados de Avalonia (UI). Define los bucles de renderizado con el Dispatcher Timer.
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
    [ObservableProperty] private bool _isPaused = false;
    [ObservableProperty] private bool _isGameOver = false;

    /// <summary>
    /// Colección reactiva de todos los elementos dibujables en pantalla (Paredes, Puntos, Fantasmas y Pac-Man).
    /// </summary>
    public ObservableCollection<EntityViewModel> GameEntities { get; } = new();

    /// <summary>
    /// Constructor del ViewModel principal del Juego. Inicializa el ciclo de vida, Models y los contadores en general.
    /// </summary>
    public PacmanGameViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _game = new GameService();
        _gameLoop = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // Aproximadamente ~60 FPS
        _gameLoop.Tick += OnGameTick;
        StartGame();
    }

    /// <summary>
    /// Llama al inicializador del backend y arranca matemáticamente el reloj del juego.
    /// </summary>
    private void StartGame()
    {
        _game.Initialize();
        SyncViewModels();
        _gameLoop.Start();
    }

    /// <summary>
    /// Convierte y traduce los objetos estrictos del Modelo (paredes, pastillas, coordenadas) a EntityViewModels puramente visuales
    /// que Avalonia puede enlazar ("Bindear") para renderizarlos en un canvas.
    /// </summary>
    private void SyncViewModels()
    {
        GameEntities.Clear();

        // Generar estáticamente las Paredes en pantalla según la matriz TileMap
        int ts = GameConstants.TileSize;
        for (int r = 0; r < _game.Map.Rows; r++)
        for (int c = 0; c < _game.Map.Cols; c++)
        {
            if (_game.Map.IsWall(r, c))
            {
                GameEntities.Add(new WallViewModel { X = c * ts, Y = r * ts });
            }
        }

        // Popular visualmente los Puntos (Dots) y Súper Píldoras (Power Pills)
        foreach (var dot in _game.Dots)
        {
            if (dot.isPower)
                GameEntities.Add(new PowerPillViewModel { X = dot.x, Y = dot.y });
            else
                GameEntities.Add(new DotViewModel { X = dot.x, Y = dot.y });
        }

        // Crear una representación visual inicial individual por cada Fantasma de la memoria lógica
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

        // Anclar la representación de Pac-Man al centro correspondiente dictado por el servicio lógico
        var playerVm = new PlayerEntityViewModel { X = _game.Player.X, Y = _game.Player.Y };
        playerVm.LoadSprite(GetPlayerSprite(_game.Player.CurrentDirection));
        GameEntities.Add(playerVm);
    }

    /// <summary>
    /// Escucha interrupciones de teclado provenientes de Avalonia y las traduce direccionalmente al servicio.
    /// </summary>
    /// <param name="key">Tecla virtual detectada por el evento.</param>
    public void HandleInput(Key key)
    {
        if (key == Key.M)
        {
            ToggleMute();
            return;
        }

        if (key == Key.Escape || key == Key.P)
        {
            TogglePause();
            return;
        }

        if (IsPaused) return; // Ignorar movimiento si el juego está en pausa

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

    /// <summary>
    /// Controlador del Timer. Le cede el flujo primero al motor de colisiones y luego actualiza todos los píxeles graficables.
    /// </summary>
    private void OnGameTick(object? sender, EventArgs e)
    {
        _game.Update();
        UpdateView();
    }

    /// <summary>
    /// Consulta recursivamente nuevas posiciones desde el Modelo y muta drásticamente lo dibujado en la ObservableCollection 
    /// para simular las animaciones fluidas cuadro por cuadro por Avalonia.
    /// </summary>
    private void UpdateView()
    {
        Score = _game.State.Score;
        HighScore = _game.State.HighScore;
        Lives = _game.State.Lives;
        
        IsGameOver = _game.State.IsGameOver;
        if (IsGameOver)
        {
            _gameLoop.Stop();
        }

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

        // Active Pickup update
        var pickupVm = GameEntities.OfType<PickupEntityViewModel>().FirstOrDefault();
        if (_game.ActivePickup != null && _game.ActivePickup.IsActive)
        {
            if (pickupVm == null)
            {
                pickupVm = new PickupEntityViewModel 
                { 
                    X = _game.ActivePickup.X, 
                    Y = _game.ActivePickup.Y, 
                    Type = _game.ActivePickup.Type 
                };
                pickupVm.LoadSprite(GetPickupSprite(_game.ActivePickup.Type));
                GameEntities.Add(pickupVm);
            }
            else if (pickupVm.Type != _game.ActivePickup.Type)
            {
                pickupVm.Type = _game.ActivePickup.Type;
                pickupVm.LoadSprite(GetPickupSprite(_game.ActivePickup.Type));
            }
        }
        else if (pickupVm != null)
        {
            GameEntities.Remove(pickupVm);
        }

        // Si la lista de la matriz lógica posterior tiene milagrosamente más puntos coleccionables que la vista, 
        // asume una transición de victoria y de siguiente nivel. Forza un re-dibujado inyectado de "SyncViewModels".
        var totalDots = _game.Dots;
        var viewDotsCount = GameEntities.Count(e => e is DotViewModel || e is PowerPillViewModel);
        if (totalDots.Count > viewDotsCount)
        {
            SyncViewModels();
            return;
        }

        // Efecto visual borrador. Itera reversivamente en la vista buscando puntos comidos y deshaciéndose graficamente de ellos.
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

    /// <summary>
    /// Recupera la imagen animada (.gif) relativa para un fantasma basado en su identidad visual o su estado de susto.
    /// </summary>
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

    /// <summary>
    /// Entrega el sprite asociado a la rotación angular física en que marcha Pac-Man temporalmente.
    /// </summary>
    private static string GetPlayerSprite(MovementDirection dir) => "PacMan.gif";

    /// <summary>
    /// Entrega el sprite asociado a la recolección especial (fruta).
    /// </summary>
    private static string GetPickupSprite(PickupType type)
    {
        return type switch
        {
            PickupType.Cherry => "Cereza.jpg",
            PickupType.Strawberry => "FreasaDeVida.jpg",
            _ => "Cereza.jpg"
        };
    }

    /// <summary>
    /// Propiedad que refleja el estado global del sonido para la UI del juego.
    /// </summary>
    public bool IsMuted => Services.LinuxSoundService.GlobalMute;

    /// <summary>
    /// Alterna el estado global de silenciamiento desde el juego.
    /// </summary>
    [RelayCommand]
    public void ToggleMute()
    {
        Services.LinuxSoundService.GlobalMute = !Services.LinuxSoundService.GlobalMute;
        OnPropertyChanged(nameof(IsMuted));
    }

    /// <summary>
    /// Alterna el estado de pausa del juego. Detiene o reanuda el Game Loop.
    /// </summary>
    [RelayCommand]
    public void TogglePause()
    {
        IsPaused = !IsPaused;
        if (IsPaused)
            _gameLoop.Stop();
        else
            _gameLoop.Start();
    }

    /// <summary>
    /// Detiene el juego y vuelve al menú principal.
    /// </summary>
    [RelayCommand]
    public void ExitGame()
    {
        _gameLoop.Stop();
        _mainViewModel.GoToMenu();
    }

    /// <summary>
    /// Reinicia todos los estados logicos y visuales para ejecutar una nueva partida posterior al Game Over.
    /// </summary>
    [RelayCommand]
    public void RestartGame()
    {
        IsGameOver = false;
        StartGame();
    }
}
