using CommunityToolkit.Mvvm.ComponentModel;
namespace PacmanGame.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;
    public MainWindowViewModel()
    {
        CurrentViewModel = new MenuViewModel(this);
    }
    public void GoToGame() => CurrentViewModel = new GameViewModel(this);
    public void GoToPacmanGame() => CurrentViewModel = new PacmanGameViewModel();
    public void GoToMenu() => CurrentViewModel = new MenuViewModel(this);
}
