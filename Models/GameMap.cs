using System;

namespace PacmanGame.Models;

public class GameMap
{
    private readonly int[,] _grid;
    public const int TileSize = 26;
    
    public int Rows { get; }
    public int Cols { get; }
    public int PixelWidth => Cols * TileSize;
    public int PixelHeight => Rows * TileSize;

    public GameMap(int[,] grid)
    {
        _grid = (int[,])grid.Clone();
        Rows = grid.GetLength(0);
        Cols = grid.GetLength(1);
    }

    public int this[int row, int col] => 
        IsInBounds(row, col) ? _grid[row, col] : 1;

    public bool IsInBounds(int row, int col) =>
        row >= 0 && row < Rows && col >= 0 && col < Cols;

    public bool IsWall(int row, int col) => this[row, col] == 1;
    
    public bool IsWalkable(int row, int col) => !IsWall(row, col);

    public bool IsGhostHouse(int row, int col) => this[row, col] == 2;

    public bool CanMoveThrough(double pixelX, double pixelY, int speed, MovementDirection dir)
    {
        var (dx, dy) = dir.ToVector();
        double nextX = pixelX + dx * speed;
        double nextY = pixelY + dy * speed;
        
        const int padding = 2;
        return CheckCorners(nextX, nextY, TileSize, padding);
    }

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

    public (int row, int col) PixelToTile(double x, double y) =>
        ((int)y / TileSize, (int)x / TileSize);
}
