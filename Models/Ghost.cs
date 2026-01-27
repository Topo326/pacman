using System;
using System.Collections.Generic;

namespace PacmanGame.Models;

public enum GhostType { Blinky, Pinky, Inky, Clyde }

public class Ghost
{
    private static readonly (int dx, int dy)[] Directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };
    private static readonly Random Rnd = new();
    
    public double X { get; set; }
    public double Y { get; set; }
    public GhostType Type { get; }
    public double ReleaseTime { get; }
    public bool IsActive { get; set; }
    public double Speed { get; set; } = 2.5; // Slower than Pacman (4)
    public MovementDirection CurrentDirection { get; set; } = MovementDirection.Left;

    public double CenterX => X + GameMap.TileSize / 2.0;
    public double CenterY => Y + GameMap.TileSize / 2.0;

    public Ghost(double x, double y, GhostType type, double releaseTime, bool startActive = false)
    {
        X = x;
        Y = y;
        Type = type;
        ReleaseTime = releaseTime;
        IsActive = startActive;
        // Initial direction usually Left for ghosts in house, or depends on setup.
        CurrentDirection = MovementDirection.Left; 
    }

    public void Update(GameMap map, Player player, Ghost? blinky)
    {
        if (!IsActive) return;

        // Check if inside Ghost House
        var (row, col) = map.PixelToTile(CenterX, CenterY);
        if (map.IsGhostHouse(row, col))
        {
            // Force move UP to exit house
            // Target is the gate or just outside: Row 11 (since house starts at 12)
            // Center of map is roughly Col 13.5
            var targetX = 13.5 * GameMap.TileSize;
            var targetY = 11 * GameMap.TileSize;
            
            // Move towards exit
            double dx = targetX - CenterX;
            double dy = targetY - CenterY;
            
            // Normalize and move
            if (Math.Abs(dx) > Speed) X += Math.Sign(dx) * Speed;
            else X = targetX - GameMap.TileSize / 2.0; // Snap X if close

            if (Math.Abs(dy) > Speed) Y += Math.Sign(dy) * Speed;
            else Y = targetY - GameMap.TileSize / 2.0;

            // Update direction for animation
            if (Math.Abs(dy) > Math.Abs(dx)) CurrentDirection = dy > 0 ? MovementDirection.Down : MovementDirection.Up;
            else CurrentDirection = dx > 0 ? MovementDirection.Right : MovementDirection.Left;
            
            return;
        }

        // 1. Move in current direction
        if (!TryMove(CurrentDirection, map))
        {
            // Hit a wall, must change direction
            CurrentDirection = GetBestDirection(map, player, blinky);
        }
        else
        {
            // 2. Check for intersection (more than 2 valid exits)
            if (IsCenteredOnTile())
            {
                var validDirections = GetValidDirections(map);
                // If it's an intersection (more than 2 ways, or just a turn)
                // In Pacman, ghosts make decisions at every tile center if there are choices.
                // We exclude 'Back' usually.
                if (validDirections.Count > 2 || (validDirections.Count == 2 && !validDirections.Contains(CurrentDirection)))
                {
                    // Choose best direction based on target
                    CurrentDirection = GetBestDirection(map, player, blinky);
                }
            }
        }
    }

    private MovementDirection GetBestDirection(GameMap map, Player player, Ghost? blinky)
    {
        var valid = GetValidDirections(map);
        var opposite = GetOpposite(CurrentDirection);
        
        // Ghosts cannot reverse direction immediately (unless forced)
        if (valid.Count > 1) valid.Remove(opposite);
        if (valid.Count == 0) return opposite;

        var target = CalculateTarget(player, blinky, map);
        
        MovementDirection bestDir = MovementDirection.None;
        double minDistance = double.MaxValue;

        foreach (var dir in valid)
        {
            var (dx, dy) = dir.ToVector();
            // Calculate position of the NEXT tile center
            // Current tile:
            var (row, col) = map.PixelToTile(CenterX, CenterY);
            double nextTileX = (col + dx) * GameMap.TileSize + GameMap.TileSize / 2.0;
            double nextTileY = (row + dy) * GameMap.TileSize + GameMap.TileSize / 2.0;

            double dist = Math.Pow(nextTileX - target.x, 2) + Math.Pow(nextTileY - target.y, 2);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestDir = dir;
            }
        }

        return bestDir;
    }

    private (double x, double y) CalculateTarget(Player player, Ghost? blinky, GameMap map)
    {
        return Type switch
        {
            GhostType.Blinky => (player.CenterX, player.CenterY),
            GhostType.Pinky => GetPinkyTarget(player),
            GhostType.Inky => GetInkyTarget(player, blinky),
            GhostType.Clyde => GetClydeTarget(player, map),
            _ => (player.CenterX, player.CenterY)
        };
    }

    private (double x, double y) GetPinkyTarget(Player player)
    {
        // 4 tiles ahead of Pacman
        int offset = GameMap.TileSize * 4;
        var (dx, dy) = player.CurrentDirection.ToVector();
        
        // Original Pacman bug: Up also goes Left (not implementing bug for now unless requested)
        return (player.CenterX + dx * offset, player.CenterY + dy * offset);
    }

    private (double x, double y) GetInkyTarget(Player player, Ghost? blinky)
    {
        if (blinky == null) return (player.CenterX, player.CenterY);
        
        // 2 tiles ahead of Pacman
        int offset = GameMap.TileSize * 2;
        var (dx, dy) = player.CurrentDirection.ToVector();
        double pivotX = player.CenterX + dx * offset;
        double pivotY = player.CenterY + dy * offset;
        
        // Vector from Blinky to Pivot
        double vecX = pivotX - blinky.CenterX;
        double vecY = pivotY - blinky.CenterY;
        
        // Double the vector
        return (blinky.CenterX + vecX * 2, blinky.CenterY + vecY * 2);
    }

    private (double x, double y) GetClydeTarget(Player player, GameMap map)
    {
        double dx = player.CenterX - CenterX;
        double dy = player.CenterY - CenterY;
        double distSq = dx * dx + dy * dy;
        double eightTilesSq = Math.Pow(GameMap.TileSize * 8, 2);
        
        // If farther than 8 tiles, chase Pacman.
        // If closer, scatter to bottom-left corner.
        if (distSq > eightTilesSq)
        {
            return (player.CenterX, player.CenterY);
        }
        else
        {
            // Scatter target: Bottom-Left corner (0, MaxRow)
            // Actually usually Clyde is Bottom-Left.
            return (0, map.PixelHeight);
        }
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
        // Top-Left
        if (!IsWalkable(x, y, map)) return false;
        // Top-Right
        if (!IsWalkable(x + GameMap.TileSize - 1, y, map)) return false;
        // Bottom-Left
        if (!IsWalkable(x, y + GameMap.TileSize - 1, map)) return false;
        // Bottom-Right
        if (!IsWalkable(x + GameMap.TileSize - 1, y + GameMap.TileSize - 1, map)) return false;

        return true;
    }

    private bool IsWalkable(double x, double y, GameMap map)
    {
        var (row, col) = map.PixelToTile(x, y);
        return map.IsWalkable(row, col);
    }

    private List<MovementDirection> GetValidDirections(GameMap map)
    {
        var list = new List<MovementDirection>();
        foreach (var dir in new[] { MovementDirection.Up, MovementDirection.Down, MovementDirection.Left, MovementDirection.Right })
        {
            var (row, col) = map.PixelToTile(CenterX, CenterY);
            var (dx, dy) = dir.ToVector();
            if (map.IsWalkable(row + dy, col + dx))
            {
                list.Add(dir);
            }
        }
        return list;
    }

    private MovementDirection GetOpposite(MovementDirection dir) => dir switch
    {
        MovementDirection.Up => MovementDirection.Down,
        MovementDirection.Down => MovementDirection.Up,
        MovementDirection.Left => MovementDirection.Right,
        MovementDirection.Right => MovementDirection.Left,
        _ => MovementDirection.None
    };

    private bool IsCenteredOnTile()
    {
        double tolerance = Speed;
        double modX = X % GameMap.TileSize;
        double modY = Y % GameMap.TileSize;
        return (modX < tolerance) && (modY < tolerance);
    }

    public void SetPosition(double x, double y)
    {
        X = x;
        Y = y;
    }
}
