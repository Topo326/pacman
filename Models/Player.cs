using System;

namespace PacmanGame.Models;

/// <summary>
/// Representa a Pacman, el personaje controlado por el jugador.
/// </summary>
public class Player
{
    public double X { get; set; }
    public double Y { get; set; }
    public int Speed { get; set; } = GameConstants.PacmanSpeed;
    public bool IsDead { get; set; }
    public MovementDirection CurrentDirection { get; private set; } = MovementDirection.None;
    public MovementDirection RequestedDirection { get; set; } = MovementDirection.None;

    public double CenterX => X + GameConstants.TileSize / 2.0;
    public double CenterY => Y + GameConstants.TileSize / 2.0;

    public Player(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Actualiza la posición de Pacman basándose en la entrada del usuario y el mapa.
    /// </summary>
    public void Update(GameMap map)
    {
        TryChangeDirection(map);
        Move(map);
    }

    private void TryChangeDirection(GameMap map)
    {
        if (RequestedDirection == MovementDirection.None) return;
        
        // Always allow 180 degree turns immediately
        if (RequestedDirection == CurrentDirection.Opposite())
        {
            CurrentDirection = RequestedDirection;
            RequestedDirection = MovementDirection.None;
            return;
        }
        
        // Only allow other turns if centered on a tile
        if (!IsCenteredOnTile()) return;

        if (!map.CanMoveThrough(X, Y, Speed, RequestedDirection)) return;
        
        // Snap to grid to prevent drifting
        SnapToGrid();
        
        CurrentDirection = RequestedDirection;
        RequestedDirection = MovementDirection.None;
    }

    private bool IsCenteredOnTile()
    {
        double epsilon = Speed;
        double modX = X % GameConstants.TileSize;
        double modY = Y % GameConstants.TileSize;
        
        bool centeredX = (modX < epsilon) || (modX > GameConstants.TileSize - epsilon);
        bool centeredY = (modY < epsilon) || (modY > GameConstants.TileSize - epsilon);
        
        return centeredX && centeredY;
    }
    
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

    private void Move(GameMap map)
    {
        if (CurrentDirection == MovementDirection.None) return;
        if (!map.CanMoveThrough(X, Y, Speed, CurrentDirection)) return;
        
        var (dx, dy) = CurrentDirection.ToVector();
        X += dx * Speed;
        Y += dy * Speed;
        
        HandleTunnels(map);
    }
    
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
    /// Restablece la posición y dirección de Pacman.
    /// </summary>
    public void Reset(double x, double y)
    {
        X = x;
        Y = y;
        IsDead = false;
        CurrentDirection = MovementDirection.None;
        RequestedDirection = MovementDirection.None;
    }
}
