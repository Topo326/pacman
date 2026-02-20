using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PacmanGame.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{

    [ObservableProperty]
    private ViewModelBase _currentView;

    public MainWindowViewModel()
    {
        CurrentView = new MenuViewModel(this);
    }

    public void StartGame()
    {
        CurrentView = new PacmanGameViewModel(this);
    }

    public void GoToMenu()
    {
        CurrentView = new MenuViewModel(this);
    }

    public void ShowHighScores()
    {
        CurrentView = new HighScoresViewModel(this);
    }
}