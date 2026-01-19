namespace PacmanGame.Models;

public class GhostData
{
    public Position Pos { get; set; }
    public string Sprite { get; set; }

    public GhostData(Position pos, string sprite)
    {
        Pos = pos;
        Sprite = sprite;
    }
}