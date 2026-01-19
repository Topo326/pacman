namespace PacmanGame.Models;

public class RandomTarget
{
    public int X { get; set; }
    public int Y { get; set; }

    public RandomTarget(int x, int y)
    {
        X = x;
        Y = y;
    }
}