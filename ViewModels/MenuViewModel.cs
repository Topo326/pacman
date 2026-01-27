using CommunityToolkit.Mvvm.Input;

namespace PacmanGame.ViewModels;

public partial class MenuViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;

    public MenuViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    [RelayCommand]
    public void StartGame()
    {
        _mainViewModel.StartGame();
    }

    [RelayCommand]
    public void ExitGame()
    {
        System.Environment.Exit(0);
    }
    
}