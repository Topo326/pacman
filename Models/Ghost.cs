using System;
using System.Collections.Generic;

namespace PacmanGame.Models;

public enum GhostType { Blinky, Pinky, Inky, Clyde }

/// <summary>
/// Representa un fantasma en el juego con su propia IA, temporizadores y estados.
/// Cada fantasma utiliza distintas estrategias euclidianas para cazar a Pac-Man basándose en su enum GhostType.
/// </summary>
public class Ghost
{
    private static readonly Random Rnd = new();
    
    public double X { get; set; }
    public double Y { get; set; }
    public GhostType Type { get; }
    public double ReleaseTime { get; set; }
    public bool IsActive { get; set; }
    public double Speed { get; set; } = GameConstants.GhostSpeed;
    public MovementDirection CurrentDirection { get; set; } = MovementDirection.Left;
    public bool IsEaten { get; set; }

    private int _lastProcessedRow = -1;
    private int _lastProcessedCol = -1;

    public double CenterX => X + GameConstants.TileSize / 2.0;
    public double CenterY => Y + GameConstants.TileSize / 2.0;

    private readonly IGhostAIStrategy _chaseStrategy;
    private readonly IGhostAIStrategy _frightenedStrategy;

    /// <summary>
    /// Constructor principal del fantasma.
    /// Define la posición inicial, su identidad, su tiempo cautivo y las estrategias de persecución asignadas.
    /// </summary>
    public Ghost(double x, double y, GhostType type, double releaseTime, bool startActive = false)
    {
        X = x;
        Y = y;
        Type = type;
        ReleaseTime = releaseTime;
        IsActive = startActive;
        CurrentDirection = MovementDirection.Left;
        
        // Asignar estrategias específicas y únicas según el tipo (personalidad del fantasma)
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
    /// Actualiza la posición y el comportamiento interno del fantasma en cada frame. 
    /// Decide su dirección tomando en cuenta paredes y calculando la menor distancia hacia la meta.
    /// </summary>
    /// <param name="map">Mapa del nivel.</param>
    /// <param name="player">Instancia de Pac-Man para usar sus coordenadas.</param>
    /// <param name="blinky">Referencia de Blinky (usado por Inky para trazar su vector).</param>
    /// <param name="isFrightened">Verdadero si Pac-Man ha comido una Píldora de Poder.</param>
    /// <param name="isScatterMode">Verdadero si los fantasmas están en su rutina global de Dispersión.</param>
    public void Update(GameMap map, Player player, Ghost? blinky, bool isFrightened, bool isScatterMode)
    {
        if (!IsActive || IsEaten) return;

        // Ajustar velocidad disminuyéndola intencionalmente si está asustado (Frightened Mode)
        Speed = isFrightened ? GameConstants.GhostFrightenedSpeed : GameConstants.GhostSpeed;

        // Lógica programada para guiar automáticamente al fantasma a la puerta y salir de la "Casa de los Fantasmas"
        var (row, col) = map.PixelToTile(CenterX, CenterY);
        if (map.IsGhostHouse(row, col))
        {
            ExitGhostHouse();
            return;
        }

        double tileX = col * GameConstants.TileSize;
        double tileY = row * GameConstants.TileSize;
        double dist = Math.Abs(X - tileX) + Math.Abs(Y - tileY);

        // Cuando la distancia actual al centro ideal de la cuadrícula es mínima, permite recalcular la dirección
        if (dist <= Speed && (_lastProcessedRow != row || _lastProcessedCol != col))
        {
            var validDirections = GetValidDirections(map);
            if (validDirections.Count != 2 || !validDirections.Contains(CurrentDirection))
            {
                _lastProcessedRow = row;
                _lastProcessedCol = col;
                X = tileX;
                Y = tileY;
                CurrentDirection = GetBestDirection(map, player, blinky, isFrightened, isScatterMode);
            }
        }

        // Si topa repentinamente o la meta dicta girar, trata de anclarse para dar una vuelta limpia de 90°
        if (!TryMove(CurrentDirection, map))
        {
            SnapToGrid();
            CurrentDirection = GetBestDirection(map, player, blinky, isFrightened, isScatterMode);
        }
    }

    /// <summary>
    /// Guía al fantasma a través de una ruta forzada en el eje Y hacia la puerta exterior de la casa central.
    /// </summary>
    private void ExitGhostHouse()
    {
        var targetX = 13.5 * GameConstants.TileSize;
        var targetY = 10 * GameConstants.TileSize;
        
        double dx = targetX - CenterX;
        double dy = targetY - CenterY;
        
        if (Math.Abs(dx) > Speed) X += Math.Sign(dx) * Speed;
        else X = targetX - GameConstants.TileSize / 2.0;

        if (Math.Abs(dy) > Speed) Y += Math.Sign(dy) * Speed;
        else Y = targetY - GameConstants.TileSize / 2.0;

        if (Math.Abs(dy) > Math.Abs(dx)) CurrentDirection = dy > 0 ? MovementDirection.Down : MovementDirection.Up;
        else CurrentDirection = dx > 0 ? MovementDirection.Right : MovementDirection.Left;
    }

    /// <summary>
    /// Analiza las intersecciones posibles y selecciona estadísticamente la mejor dirección para el próximo movimiento.
    /// </summary>
    /// <returns>La mejor nueva dirección (MovementDirection) hacia la celda Target calculada por distancia al cuadrado.</returns>
    private MovementDirection GetBestDirection(GameMap map, Player player, Ghost? blinky, bool isFrightened, bool isScatterMode)
    {
        var valid = GetValidDirections(map);
        var opposite = CurrentDirection.Opposite();
        
        // Evitar que el fantasma gire en 'U' espontáneamente, regla del arcade original (salvo en ciertos cambios de modo)
        if (valid.Count > 1) valid.Remove(opposite);
        if (valid.Count == 0) return opposite;

        // Seleccionar la lógica dinámica de inteligencia correspondiente en base al estado del juego
        var strategy = isFrightened ? _frightenedStrategy : _chaseStrategy;
        var target = strategy.GetTarget(this, player, blinky, map, isScatterMode);
        
        MovementDirection bestDir = MovementDirection.None;
        double minDistance = double.MaxValue;

        foreach (var dir in valid)
        {
            var (dx, dy) = dir.ToVector();
            var (row, col) = map.PixelToTile(CenterX, CenterY);
            double nextTileX = (col + dx) * GameConstants.TileSize + GameConstants.TileSize / 2.0;
            double nextTileY = (row + dy) * GameConstants.TileSize + GameConstants.TileSize / 2.0;

            // Análisis de Distancia Euclidiana (al cuadrado para ahorrar costos de raíz) para buscar el camino directo
            double dist = Math.Pow(nextTileX - target.x, 2) + Math.Pow(nextTileY - target.y, 2);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestDir = dir;
            }
        }

        return bestDir;
    }

    /// <summary>
    /// Mueve progresivamente al fantasma añadiendo su velocidad vectorial a su posición espacial.
    /// </summary>
    /// <returns>True si el muro estaba libre y avanzó, o False si hay pared interrumpiéndolo.</returns>
    private bool TryMove(MovementDirection direction, GameMap map)
    {
        var (dx, dy) = direction.ToVector();
        double nextX = X + dx * Speed;
        double nextY = Y + dy * Speed;

        if (CanMoveTo(nextX, nextY, map))
        {
            X = nextX;
            Y = nextY;
            HandleTunnels(map);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Comprueba si el fantasma ha tocado los píxeles marginales de los márgenes derecho o izquierdo, 
    /// reubicándolo cíclicamente al extremo opuesto imitando el efecto Pac-Man infinito.
    /// </summary>
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
    /// Valida usando cuatro puntos cardinales alrededor del Sprite Box que la nueva coordenada no se interponga con paredes sólidas.
    /// </summary>
    private bool CanMoveTo(double x, double y, GameMap map)
    {
        int ts = GameConstants.TileSize;
        return IsWalkable(x, y, map) && 
               IsWalkable(x + ts - 1, y, map) && 
               IsWalkable(x, y + ts - 1, map) && 
               IsWalkable(x + ts - 1, y + ts - 1, map);
    }

    /// <summary>
    /// Convierte la coordenada píxel en un subíndice (fila/col) para consultar su existencia en la matriz Walkable del mapa.
    /// </summary>
    private bool IsWalkable(double x, double y, GameMap map)
    {
        var (row, col) = map.PixelToTile(x, y);
        return map.IsWalkable(row, col);
    }

    /// <summary>
    /// Genera una lista completa que contiene únicamente las direcciones perimetrales accesibles que un fantasma podría tomar desde su posición de bloque actual.
    /// </summary>
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

    /// <summary>
    /// Ajusta bruscamente las coordenadas físicas del fantasma en X y Y para que encajen limpios en la cuadrícula sin sobrepasar floats decimales.
    /// </summary>
    private void SnapToGrid()
    {
        X = Math.Round(X / GameConstants.TileSize) * GameConstants.TileSize;
        Y = Math.Round(Y / GameConstants.TileSize) * GameConstants.TileSize;
    }

    /// <summary>
    /// Reinicia completamente las propiedades esenciales del fantasma para prepararlo en su punto de origen o posterior a una muerte.
    /// </summary>
    public void Reset(double x, double y)
    {
        X = x;
        Y = y;
        IsEaten = false;
        CurrentDirection = MovementDirection.Left;
        _lastProcessedRow = -1;
        _lastProcessedCol = -1;
    }
}
