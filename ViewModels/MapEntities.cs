using PacmanGame.Models;

namespace PacmanGame.ViewModels;

public class WallViewModel : EntityViewModel { } 
public class DotViewModel : EntityViewModel { }  
public class PowerPillViewModel : EntityViewModel { }
public class PlayerEntityViewModel : EntityViewModel { } 

public class GhostEntityViewModel : EntityViewModel
{
    public GhostType Type { get; set; }
    public bool IsActive { get; set; } 
    public bool IsFrightened { get; set; }
}
