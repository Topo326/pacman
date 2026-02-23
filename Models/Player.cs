using System;

namespace PacmanGame.Models;

/// <summary>
/// Representa a Pac-Man, el personaje principal controlado por el jugador.
/// Gestiona la posición, velocidad, dirección y la lógica de movimiento a través de la cuadrícula.
/// </summary>
public class Player
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; } = GameConstants.PacmanSpeed;
    public bool IsDead { get; set; }
    public MovementDirection CurrentDirection { get; private set; } = MovementDirection.None;
    public MovementDirection RequestedDirection { get; set; } = MovementDirection.None;

    public double CenterX => X + GameConstants.TileSize / 2.0;
    public double CenterY => Y + GameConstants.TileSize / 2.0;

    /// <summary>
    /// Constructor principal de Pac-Man.
    /// </summary>
    /// <param name="x">Posición inicial en el eje X (píxeles).</param>
    /// <param name="y">Posición inicial en el eje Y (píxeles).</param>
    public Player(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Actualiza la posición de Pac-Man en cada frame, comprobando la entrada del usuario y colisiones en el mapa.
    /// </summary>
    /// <param name="map">Mapa actual del laberinto para validar muros e intersecciones.</param>
    public void Update(GameMap map)
    {
        TryChangeDirection(map);
        Move(map);
    }

    /// <summary>
    /// Verifica si Pac-Man puede girar hacia la dirección solicitada por el jugador.
    /// </summary>
    /// <param name="map">Mapa del laberinto para verificar paredes.</param>
    private void TryChangeDirection(GameMap map)
    {
        if (RequestedDirection == MovementDirection.None) return;
        
        // Siempre permite giros de 180 grados inmediatamente sin esperar a estar centrado
        if (RequestedDirection == CurrentDirection.Opposite())
        {
            CurrentDirection = RequestedDirection;
            RequestedDirection = MovementDirection.None;
            return;
        }
        
        // Solo permite giros de 90 grados si Pac-Man está centrado geométricamente en la baldosa
        if (!IsCenteredOnTile()) return;

        if (!map.CanMoveThrough(X, Y, Speed, RequestedDirection)) return;
        
        // Ajustar al centro de la cuadrícula para evitar desvíos microscópicos (drifting)
        SnapToGrid();
        
        CurrentDirection = RequestedDirection;
        RequestedDirection = MovementDirection.None;
    }

    /// <summary>
    /// Calcula matemáticamente si Pac-Man está lo suficientemente cerca del centro de una baldosa para girar limpiamente.
    /// </summary>
    /// <returns>True si está centrado para girar, False en caso contrario.</returns>
    private bool IsCenteredOnTile()
    {
        double epsilon = Speed;
        double modX = X % GameConstants.TileSize;
        double modY = Y % GameConstants.TileSize;
        
        bool centeredX = (modX < epsilon) || (modX > GameConstants.TileSize - epsilon);
        bool centeredY = (modY < epsilon) || (modY > GameConstants.TileSize - epsilon);
        
        return centeredX && centeredY;
    }
    
    /// <summary>
    /// Alinea forzosamente las coordenadas de Pac-Man a los límites exactos de la baldosa actual 
    /// para asegurar que al avanzar no muerda colisiones de muros laterales.
    /// </summary>
    private void SnapToGrid()
    {
        double modX = X % GameConstants.TileSize;
        if (modX < Speed || modX > GameConstants.TileSize - Speed)
        {
            X = Math.Round(X / GameConstants.TileSize) * GameConstants.TileSize;
        }
        
        double modY = Y % GameConstants.TileSize;
        if (modY < Speed || modY > GameConstants.TileSize - Speed)
        {
            Y = Math.Round(Y / GameConstants.TileSize) * GameConstants.TileSize;
        }
    }

    /// <summary>
    /// Mueve a Pac-Man espacialmente si la dirección actual no topa con una pared.
    /// </summary>
    /// <param name="map">El mapa del juego para confirmar rutas caminables.</param>
    private void Move(GameMap map)
    {
        if (CurrentDirection == MovementDirection.None) return;
        if (!map.CanMoveThrough(X, Y, Speed, CurrentDirection)) return;
        
        var (dx, dy) = CurrentDirection.ToVector();
        X += dx * Speed;
        Y += dy * Speed;
        
        HandleTunnels(map);
    }
    
    /// <summary>
    /// Detecta e interactúa con los túneles a los extremos izquierdo y derecho del laberinto.
    /// </summary>
    /// <param name="map">El mapa para conocer los anchos permitidos totales.</param>
    private void HandleTunnels(GameMap map)
    {
        double rightEdge = map.PixelWidth;
        if (X < -GameConstants.TileSize / 2.0)
        {
            X = rightEdge + GameConstants.TileSize / 2.0;
        }
        else if (X > rightEdge + GameConstants.TileSize / 2.0)
        {
            X = -GameConstants.TileSize / 2.0;
        }
    }

    /// <summary>
    /// Restablece la posición y dirección de Pac-Man posterior a la pérdida de una vida o inicio del nivel.
    /// </summary>
    /// <param name="x">Posición inicial predeterminada en X.</param>
    /// <param name="y">Posición inicial predeterminada en Y.</param>
    public void Reset(double x, double y)
    {
        X = x;
        Y = y;
        IsDead = false;
        CurrentDirection = MovementDirection.None;
        RequestedDirection = MovementDirection.None;
    }
}
