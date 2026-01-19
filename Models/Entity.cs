namespace PacmanGame.Models;

public abstract class Entity
{
    public Position Pos { get; set; }
    public int Dx { get; set; }
    public int Dy { get; set; }
    public int SpeedTilesPerTick { get; set; } = 1;

    protected Entity(Position pos)
    {
        Pos = pos;
        Dx = 0;
        Dy = 0;
    }

    public Position NextTile() => Pos.Offset(Dx, Dy);

    public void Step()
    {
        Pos = Pos.Offset(Dx * SpeedTilesPerTick, Dy * SpeedTilesPerTick);
    }
}

public class PlayerEntity : Entity
{
    public int DesiredDx { get; set; }
    public int DesiredDy { get; set; }

    public PlayerEntity(Position pos) : base(pos) { }
}
