using PacmanGame.Models;

namespace PacmanGame.ViewModels;

public class WallViewModel : EntityViewModel { } // Para paredes
public class DotViewModel : EntityViewModel { }  // Para puntos
public class PlayerEntityViewModel : EntityViewModel { } // Para Pac-Man

public class GhostEntityViewModel : EntityViewModel
{
    public GhostType Type { get; set; }
    public double ReleaseTime { get; set; } // Tiempo en segundos para liberar
    public bool IsActive { get; set; } // Si ya fue liberado
}
