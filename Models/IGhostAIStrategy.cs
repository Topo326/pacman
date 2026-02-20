namespace PacmanGame.Models;

/// <summary>
/// Define la interfaz para las estrategias de movimiento de los fantasmas.
/// </summary>
public interface IGhostAIStrategy
{
    (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map);
}

/// <summary>
/// Estrategia para el comportamiento de Blinky (persigue directamente a Pacman).
/// </summary>
public class BlinkyChaseStrategy : IGhostAIStrategy
{
    public (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map) 
        => (player.CenterX, player.CenterY);
}

/// <summary>
/// Estrategia para el comportamiento de Pinky (apunta delante de Pacman).
/// </summary>
public class PinkyChaseStrategy : IGhostAIStrategy
{
    public (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map)
    {
        int offset = GameConstants.TileSize * 4;
        var (dx, dy) = player.CurrentDirection.ToVector();
        return (player.CenterX + dx * offset, player.CenterY + dy * offset);
    }
}

/// <summary>
/// Estrategia para el comportamiento de Inky (usa la posición de Blinky y Pacman).
/// </summary>
public class InkyChaseStrategy : IGhostAIStrategy
{
    public (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map)
    {
        if (blinky == null) return (player.CenterX, player.CenterY);
        
        int offset = GameConstants.TileSize * 2;
        var (dx, dy) = player.CurrentDirection.ToVector();
        double pivotX = player.CenterX + dx * offset;
        double pivotY = player.CenterY + dy * offset;
        
        double vecX = pivotX - blinky.CenterX;
        double vecY = pivotY - blinky.CenterY;
        
        return (blinky.CenterX + vecX * 2, blinky.CenterY + vecY * 2);
    }
}

/// <summary>
/// Estrategia para el comportamiento de Clyde (persigue si está lejos, huye si está cerca).
/// </summary>
public class ClydeChaseStrategy : IGhostAIStrategy
{
    public (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map)
    {
        double dx = player.CenterX - ghost.CenterX;
        double dy = player.CenterY - ghost.CenterY;
        double distSq = dx * dx + dy * dy;
        double eightTilesSq = Math.Pow(GameConstants.TileSize * 8, 2);
        
        if (distSq > eightTilesSq)
            return (player.CenterX, player.CenterY);
        
        return (0, map.PixelHeight); // Esquina inferior izquierda
    }
}

/// <summary>
/// Estrategia para cuando los fantasmas están asustados (huyen a sus esquinas).
/// </summary>
public class FrightenedStrategy : IGhostAIStrategy
{
    public (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map)
    {
        return ghost.Type switch
        {
            GhostType.Blinky => (map.PixelWidth, 0), // Top Right
            GhostType.Pinky => (0, 0), // Top Left
            GhostType.Inky => (map.PixelWidth, map.PixelHeight), // Bottom Right
            GhostType.Clyde => (0, map.PixelHeight), // Bottom Left
            _ => (0, 0)
        };
    }
}
