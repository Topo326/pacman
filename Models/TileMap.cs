using System;

namespace PacmanGame.Models;

public class TileMap
{
    private readonly int[,] map;
    public int Width { get; }
    public int Height { get; }

    public TileMap(int[,] map)
    {
        this.map = map ?? throw new ArgumentNullException(nameof(map));
        Height = map.GetLength(0);
        Width = map.GetLength(1);
    }

    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return false;
        return map[y, x] == 0;
    }

    public int GetTile(int x, int y) => (x < 0 || y < 0 || x >= Width || y >= Height) ? 1 : map[y, x];
}

public class Tile
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Sprite { get; set; }

    public Tile(int x, int y, string sprite)
    {
        X = x;
        Y = y;
        Sprite = sprite;
    }
}
