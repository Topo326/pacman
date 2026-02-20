using PacmanGame.Models;

namespace PacmanGame.ViewModels;

public class WallViewModel : EntityViewModel { } 
public class DotViewModel : EntityViewModel { }  
public class PowerPillViewModel : EntityViewModel { }
public partial class PlayerEntityViewModel : EntityViewModel 
{ 
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _isDead;
} 

public class GhostEntityViewModel : EntityViewModel
{
    public GhostType Type { get; set; }
    public bool IsActive { get; set; } 
    public bool IsFrightened { get; set; }
}
