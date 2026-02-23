using System;

namespace PacmanGame.Models;

/// <summary>
/// Define la interfaz para las estrategias de movimiento e inteligencia artificial de los fantasmas.
/// Cada fantasma implementa esta interfaz para decidir su objetivo (Tile) al que moverse.
/// </summary>
public interface IGhostAIStrategy
{
    /// <summary>
    /// Calcula y devuelve la coordenada objetivo (Target) en píxeles a la que el fantasma intentará dirigirse.
    /// </summary>
    /// <param name="ghost">La instancia del fantasma que está calculando su estrategia.</param>
    /// <param name="player">El jugador (Pac-Man) actual para basarse en su posición.</param>
    /// <param name="blinky">Referencia a Blinky (necesaria para la estrategia de Inky).</param>
    /// <param name="map">El mapa actual del juego.</param>
    /// <param name="isScatterMode">Indica si el fantasma está en modo de dispersión (Scatter) hacia su esquina.</param>
    /// <returns>Una tupla con las coordenadas (x, y) del píxel objetivo.</returns>
    (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map, bool isScatterMode);
}

/// <summary>
/// Estrategia para el comportamiento de Blinky (Fantasma Rojo).
/// En modo Persecución (Chase), Blinky persigue agresivamente y directamente la posición exacta de Pac-Man.
/// </summary>
public class BlinkyChaseStrategy : IGhostAIStrategy
{
    public (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map, bool isScatterMode) 
    {
        if (isScatterMode) return (map.PixelWidth, -GameConstants.TileSize); // Arriba Derecha
        return (player.CenterX, player.CenterY);
    }
}

/// <summary>
/// Estrategia para el comportamiento de Pinky (Fantasma Rosa).
/// En modo Persecución, Pinky intenta emboscar a Pac-Man apuntando a 4 baldosas (tiles) por delante de la dirección actual de Pac-Man.
/// </summary>
public class PinkyChaseStrategy : IGhostAIStrategy
{
    public (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map, bool isScatterMode)
    {
        if (isScatterMode) return (-GameConstants.TileSize, -GameConstants.TileSize); // Arriba Izquierda
        
        int offset = GameConstants.TileSize * 4;
        var (dx, dy) = player.CurrentDirection.ToVector();
        return (player.CenterX + dx * offset, player.CenterY + dy * offset);
    }
}

/// <summary>
/// Estrategia para el comportamiento de Inky (Fantasma Cyan).
/// En modo Persecución, Inky usa un objetivo complejo vectorial: toma una posición 2 baldosas por delante de Pac-Man y luego traza un vector desde Blinky hasta ese punto, duplicando esa distancia para acorralar.
/// </summary>
public class InkyChaseStrategy : IGhostAIStrategy
{
    public (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map, bool isScatterMode)
    {
        if (isScatterMode) return (map.PixelWidth, map.PixelHeight + GameConstants.TileSize); // Abajo Derecha
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
/// Estrategia para el comportamiento de Clyde (Fantasma Naranja).
/// En modo Persecución, Clyde actúa como Blinky si está a más de 8 baldosas de Pac-Man. Sin embargo, si se acerca a menos de 8 baldosas, aborta la persecución y huye a su esquina de dispersión.
/// </summary>
public class ClydeChaseStrategy : IGhostAIStrategy
{
    public (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map, bool isScatterMode)
    {
        if (isScatterMode) return (-GameConstants.TileSize, map.PixelHeight + GameConstants.TileSize); // Abajo Izquierda
        
        double dx = player.CenterX - ghost.CenterX;
        double dy = player.CenterY - ghost.CenterY;
        double distSq = dx * dx + dy * dy;
        double eightTilesSq = Math.Pow(GameConstants.TileSize * 8, 2);
        
        if (distSq > eightTilesSq)
            return (player.CenterX, player.CenterY);
        
        return (-GameConstants.TileSize, map.PixelHeight + GameConstants.TileSize); // Huye hacia su respectiva esquina de dispersión
    }
}

/// <summary>
/// Estrategia global asustada (Frightened Mode) para todos los fantasmas.
/// Cuando Pac-Man come una Power Pill (Píldora de Poder), los fantasmas se vuelven azules e intentan huir fijando su objetivo de vuelta en su respectiva esquina de origen.
/// </summary>
public class FrightenedStrategy : IGhostAIStrategy
{
    public (double x, double y) GetTarget(Ghost ghost, Player player, Ghost? blinky, GameMap map, bool isScatterMode)
    {
        return ghost.Type switch
        {
            GhostType.Blinky => (map.PixelWidth, 0), // Arriba Derecha
            GhostType.Pinky => (0, 0), // Arriba Izquierda
            GhostType.Inky => (map.PixelWidth, map.PixelHeight), // Abajo Derecha
            GhostType.Clyde => (0, map.PixelHeight), // Abajo Izquierda
            _ => (0, 0)
        };
    }
}
