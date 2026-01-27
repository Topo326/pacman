namespace PacmanGame.Models;

public class Tile
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Sprite { get; set; } = string.Empty;

    public Tile(int x, int y, string sprite)
    {
        X = x;
        Y = y;
        Sprite = sprite;
    }
}
