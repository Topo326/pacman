namespace PacmanGame.Models;

public enum MovementDirection
{
    None = 0,
    Up,
    Down,
    Left,
    Right
}

public static class DirectionExtensions
{
    public static (int dx, int dy) ToVector(this MovementDirection dir) => dir switch
    {
        MovementDirection.Up => (0, -1),
        MovementDirection.Down => (0, 1),
        MovementDirection.Left => (-1, 0),
        MovementDirection.Right => (1, 0),
        _ => (0, 0)
    };
    
    public static MovementDirection Opposite(this MovementDirection dir) => dir switch
    {
        MovementDirection.Up => MovementDirection.Down,
        MovementDirection.Down => MovementDirection.Up,
        MovementDirection.Left => MovementDirection.Right,
        MovementDirection.Right => MovementDirection.Left,
        _ => MovementDirection.None
    };
}
