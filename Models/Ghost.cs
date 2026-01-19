namespace PacmanGame.Models;

public enum GhostType { Blinky, Pinky, Inky, Clyde }

public class GhostEntity : Entity
{
    public GhostType Type { get; }
    private static readonly (int dx, int dy)[] Neighbours = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };

    public GhostEntity(Position pos, GhostType type) : base(pos)
    {
        Type = type;
    }

    public Position ComputeTarget(Position pacmanPos, int pacmanDx, int pacmanDy, Position blinkyPos, TileMap map)
    {
        switch (Type)
        {
            case GhostType.Blinky:
                return pacmanPos;
            case GhostType.Pinky:
                return pacmanPos.Offset(pacmanDx * 4, pacmanDy * 4);
            case GhostType.Inky:
            {
                var ahead = pacmanPos.Offset(pacmanDx * 2, pacmanDy * 2);
                var vecX = ahead.X - blinkyPos.X;
                var vecY = ahead.Y - blinkyPos.Y;
                return new Position(blinkyPos.X + vecX * 2, blinkyPos.Y + vecY * 2);
            }
            case GhostType.Clyde:
            {
                var d = Pos.DistanceTo(pacmanPos);
                return d > 8 ? pacmanPos : new Position(1, map.Height - 2);
            }
            default:
                return pacmanPos;
        }
    }

    public void ChooseDirectionTowards(Position target, TileMap map)
    {
        double best = double.MaxValue;
        int bestDx = 0, bestDy = 0;
        foreach (var (dx, dy) in Neighbours)
        {
            var nx = Pos.X + dx;
            var ny = Pos.Y + dy;
            if (!map.IsWalkable(nx, ny)) continue;
            var dist = new Position(nx, ny).DistanceTo(target);
            if (dist < best)
            {
                best = dist;
                bestDx = dx; bestDy = dy;
            }
        }
        Dx = bestDx; Dy = bestDy;
    }

    public void Move()
    {
        // Implement movement logic for the ghost
        // For now, we can leave this as a placeholder
    }
}
