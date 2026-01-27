namespace PacmanGame.Models;

public class Player
{
    public double X { get; set; }
    public double Y { get; set; }
    public int Speed { get; set; } = 2;
    public MovementDirection CurrentDirection { get; private set; } = MovementDirection.None;
    public MovementDirection RequestedDirection { get; set; } = MovementDirection.None;

    public double CenterX => X + GameMap.TileSize / 2.0;
    public double CenterY => Y + GameMap.TileSize / 2.0;

    public Player(double x, double y)
    {
        X = x;
        Y = y;
    }

    public void Update(GameMap map)
    {
        TryChangeDirection(map);
        Move(map);
    }

    private void TryChangeDirection(GameMap map)
    {
        if (RequestedDirection == MovementDirection.None) return;
        if (!map.CanMoveThrough(X, Y, Speed, RequestedDirection)) return;
        
        CurrentDirection = RequestedDirection;
        RequestedDirection = MovementDirection.None;
    }

    private void Move(GameMap map)
    {
        if (CurrentDirection == MovementDirection.None) return;
        if (!map.CanMoveThrough(X, Y, Speed, CurrentDirection)) return;
        
        var (dx, dy) = CurrentDirection.ToVector();
        X += dx * Speed;
        Y += dy * Speed;
    }

    public void SetPosition(double x, double y)
    {
        X = x;
        Y = y;
        CurrentDirection = MovementDirection.None;
        RequestedDirection = MovementDirection.None;
    }
}
