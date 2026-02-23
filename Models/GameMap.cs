using System;

namespace PacmanGame.Models;

/// <summary>
/// Representa matemáticamente el mapa del juego en una matriz 2D. 
/// Proporciona la lógica integral para validación de colisiones, límites y navegación espacial de las entidades.
/// </summary>
public class GameMap
{
    private readonly int[,] _grid;
    
    /// <summary>
    /// Tamaño dimensionado de cada baldosa del mapa en píxeles.
    /// </summary>
    public int TileSize => GameConstants.TileSize;
    
    /// <summary>
    /// Cantidad total de filas del laberinto.
    /// </summary>
    public int Rows { get; }
    
    /// <summary>
    /// Cantidad total de columnas del laberinto.
    /// </summary>
    public int Cols { get; }
    
    /// <summary>
    /// Ancho total en píxeles calculado dinámicamente.
    /// </summary>
    public int PixelWidth => Cols * TileSize;
    
    /// <summary>
    /// Altura total en píxeles calculada dinámicamente.
    /// </summary>
    public int PixelHeight => Rows * TileSize;

    /// <summary>
    /// Constructor principal que clona la cuadrícula para evitar mutaciones externas del mapa original.
    /// </summary>
    /// <param name="grid">Matriz bidimensional de enteros que define el nivel.</param>
    public GameMap(int[,] grid)
    {
        _grid = (int[,])grid.Clone();
        Rows = grid.GetLength(0);
        Cols = grid.GetLength(1);
    }

    /// <summary>
    /// Indexador bidimensional que permite acceder al tipo de tile de forma segura gestionando los bordes fuera de los límites como paredes opacas.
    /// </summary>
    public int this[int row, int col] => 
        IsInBounds(row, col) ? _grid[row, col] : 1;

    /// <summary>
    /// Confirma si las coordenadas consultadas pertenecen a las matrices internas del mapa.
    /// </summary>
    public bool IsInBounds(int row, int col) =>
        row >= 0 && row < Rows && col >= 0 && col < Cols;

    /// <summary>
    /// Devuelve True si la baldosa es una pared de límite (Sólida - Valor 1).
    /// </summary>
    public bool IsWall(int row, int col) => this[row, col] == 1;
    
    /// <summary>
    /// Devuelve True si una entidad terrestre puede transitar por esta baldosa (Pasillo Libre - Valor != 1).
    /// </summary>
    public bool IsWalkable(int row, int col) => !IsWall(row, col);

    /// <summary>
    /// Devuelve True si la celda referida conforma el encierro o puerta principal asimétrica de la casa de los fantasmas (Valor 2).
    /// </summary>
    public bool IsGhostHouse(int row, int col) => this[row, col] == 2;

    /// <summary>
    /// Devuelve True si la celda analizada guarda una Píldora de Poder grande (Valor 3).
    /// </summary>
    public bool IsPowerPill(int row, int col) => this[row, col] == 3;

    /// <summary>
    /// Calcula proactivamente a un frame de distancia si un cuerpo (en base a su velocidad y radio) es capaz de trasladarse hacia una nueva posición sin cruzar un muro sólido.
    /// </summary>
    /// <param name="pixelX">El anclaje actual X de la entidad.</param>
    /// <param name="pixelY">El anclaje actual Y de la entidad.</param>
    /// <param name="speed">La velocidad instantánea del objeto.</param>
    /// <param name="dir">El vector direccional para mover el cuerpo matemático.</param>
    /// <returns>True si es un movimiento físico válido contorneado.</returns>
    public bool CanMoveThrough(double pixelX, double pixelY, double speed, MovementDirection dir)
    {
        var (dx, dy) = dir.ToVector();
        double nextX = pixelX + dx * speed;
        double nextY = pixelY + dy * speed;
        
        // Define un recuadro tolerante a fallos para redondear bordes filosos del laberinto
        const int padding = 2;
        return CheckCorners(nextX, nextY, TileSize, padding);
    }

    /// <summary>
    /// Traza cuatro puntos colisionadores (AABB - Axis-Aligned Bounding Box marginado) para comprobar que la figura abstracta de la entidad nunca intercepte celdas sólidas (1).
    /// </summary>
    private bool CheckCorners(double x, double y, int size, int padding)
    {
        int[] cornersX = { (int)(x + padding), (int)(x + size - padding) };
        int[] cornersY = { (int)(y + padding), (int)(y + size - padding) };

        foreach (int cx in cornersX)
        foreach (int cy in cornersY)
        {
            int col = cx / TileSize;
            int row = cy / TileSize;
            if (IsWall(row, col)) return false;
        }
        return true;
    }

    /// <summary>
    /// Convierte y trunca analíticamente las coordenadas del plano global de la pantalla (píxeles) a índices abstractos y discretos de matriz 2D.
    /// </summary>
    /// <returns>Una tupla de indexación [fila, columna] (Y, X).</returns>
    public (int row, int col) PixelToTile(double x, double y) =>
        ((int)y / TileSize, (int)x / TileSize);
}
