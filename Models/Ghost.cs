using System;
using System.Collections.Generic;

namespace PacmanGame.Models;

public enum GhostType { Blinky, Pinky, Inky, Clyde }

/// <summary>
/// Representa un fantasma en el juego con su propia IA y estados.
/// </summary>
public class Ghost
{
    private static readonly Random Rnd = new();
    
    public double X { get; set; }
    public double Y { get; set; }
    public GhostType Type { get; }
    public double ReleaseTime { get; }
    public bool IsActive { get; set; }
    public double Speed { get; set; } = GameConstants.GhostSpeed;
    public MovementDirection CurrentDirection { get; set; } = MovementDirection.Left;
    public bool IsEaten { get; set; }

    public double CenterX => X + GameConstants.TileSize / 2.0;
    public double CenterY => Y + GameConstants.TileSize / 2.0;

    private readonly IGhostAIStrategy _chaseStrategy;
    private readonly IGhostAIStrategy _frightenedStrategy;

    public Ghost(double x, double y, GhostType type, double releaseTime, bool startActive = false)
    {
        X = x;
        Y = y;
        Type = type;
        ReleaseTime = releaseTime;
        IsActive = startActive;
        CurrentDirection = MovementDirection.Left;
        
        // Asignar estrategias según el tipo
        _chaseStrategy = type switch
        {
            GhostType.Blinky => new BlinkyChaseStrategy(),
            GhostType.Pinky => new PinkyChaseStrategy(),
            GhostType.Inky => new InkyChaseStrategy(),
            GhostType.Clyde => new ClydeChaseStrategy(),
            _ => new BlinkyChaseStrategy()
        };
        _frightenedStrategy = new FrightenedStrategy();
    }

    /// <summary>
    /// Actualiza la posición y el comportamiento del fantasma.
    /// </summary>
    public void Update(GameMap map, Player player, Ghost? blinky, bool isFrightened)
    {
        if (!IsActive || IsEaten) return;

        // Ajustar velocidad si está asustado
        Speed = isFrightened ? GameConstants.GhostFrightenedSpeed : GameConstants.GhostSpeed;

        // Lógica para salir de la casa de los fantasmas
        var (row, col) = map.PixelToTile(CenterX, CenterY);
        if (map.IsGhostHouse(row, col))
        {
            ExitGhostHouse();
            return;
        }

        // Movimiento normal
        if (!TryMove(CurrentDirection, map))
        {
            CurrentDirection = GetBestDirection(map, player, blinky, isFrightened);
        }
        else if (IsCenteredOnTile())
        {
            var validDirections = GetValidDirections(map);
            if (validDirections.Count > 2 || (validDirections.Count == 2 && !validDirections.Contains(CurrentDirection)))
            {
                CurrentDirection = GetBestDirection(map, player, blinky, isFrightened);
            }
        }
    }

    private void ExitGhostHouse()
    {
        var targetX = 13.5 * GameConstants.TileSize;
        var targetY = 11 * GameConstants.TileSize;
        
        double dx = targetX - CenterX;
        double dy = targetY - CenterY;
        
        if (Math.Abs(dx) > Speed) X += Math.Sign(dx) * Speed;
        else X = targetX - GameConstants.TileSize / 2.0;

        if (Math.Abs(dy) > Speed) Y += Math.Sign(dy) * Speed;
        else Y = targetY - GameConstants.TileSize / 2.0;

        if (Math.Abs(dy) > Math.Abs(dx)) CurrentDirection = dy > 0 ? MovementDirection.Down : MovementDirection.Up;
        else CurrentDirection = dx > 0 ? MovementDirection.Right : MovementDirection.Left;
    }

    private MovementDirection GetBestDirection(GameMap map, Player player, Ghost? blinky, bool isFrightened)
    {
        var valid = GetValidDirections(map);
        var opposite = CurrentDirection.Opposite();
        
        if (valid.Count > 1) valid.Remove(opposite);
        if (valid.Count == 0) return opposite;

        // Usar la estrategia correspondiente
        var strategy = isFrightened ? _frightenedStrategy : _chaseStrategy;
        var target = strategy.GetTarget(this, player, blinky, map);
        
        MovementDirection bestDir = MovementDirection.None;
        double minDistance = double.MaxValue;

        foreach (var dir in valid)
        {
            var (dx, dy) = dir.ToVector();
            var (row, col) = map.PixelToTile(CenterX, CenterY);
            double nextTileX = (col + dx) * GameConstants.TileSize + GameConstants.TileSize / 2.0;
            double nextTileY = (row + dy) * GameConstants.TileSize + GameConstants.TileSize / 2.0;

            double dist = Math.Pow(nextTileX - target.x, 2) + Math.Pow(nextTileY - target.y, 2);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestDir = dir;
            }
        }

        return bestDir;
    }

    private bool TryMove(MovementDirection direction, GameMap map)
    {
        var (dx, dy) = direction.ToVector();
        double nextX = X + dx * Speed;
        double nextY = Y + dy * Speed;

        if (CanMoveTo(nextX, nextY, map))
        {
            X = nextX;
            Y = nextY;
            return true;
        }
        return false;
    }

    private bool CanMoveTo(double x, double y, GameMap map)
    {
        int ts = GameConstants.TileSize;
        return IsWalkable(x, y, map) && 
               IsWalkable(x + ts - 1, y, map) && 
               IsWalkable(x, y + ts - 1, map) && 
               IsWalkable(x + ts - 1, y + ts - 1, map);
    }

    private bool IsWalkable(double x, double y, GameMap map)
    {
        var (row, col) = map.PixelToTile(x, y);
        return map.IsWalkable(row, col);
    }

    private List<MovementDirection> GetValidDirections(GameMap map)
    {
        var list = new List<MovementDirection>();
        var directions = new[] { MovementDirection.Up, MovementDirection.Down, MovementDirection.Left, MovementDirection.Right };
        var (row, col) = map.PixelToTile(CenterX, CenterY);

        foreach (var dir in directions)
        {
            var (dx, dy) = dir.ToVector();
            if (map.IsWalkable(row + dy, col + dx))
            {
                list.Add(dir);
            }
        }
        return list;
    }

    private bool IsCenteredOnTile()
    {
        double tolerance = Speed;
        double modX = X % GameConstants.TileSize;
        double modY = Y % GameConstants.TileSize;
        return (modX < tolerance) && (modY < tolerance);
    }

    public void Reset(double x, double y)
    {
        X = x;
        Y = y;
        IsEaten = false;
        CurrentDirection = MovementDirection.Left;
    }
}
