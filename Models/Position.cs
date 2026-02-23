using System;

namespace PacmanGame.Models;

public record Position(int X, int Y)
{
    public double DistanceTo(Position other) => Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));
    public Position Offset(int dx, int dy) => new Position(X + dx, Y + dy);
}
