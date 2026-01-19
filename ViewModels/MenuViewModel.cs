using CommunityToolkit.Mvvm.Input;

namespace PacmanGame.ViewModels;
partial class MenuViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVM;
    public MenuViewModel(MainWindowViewModel mainVM)
    {
        _mainVM = mainVM;
    }
    [RelayCommand]
    private void StartGame()
    {
        _mainVM.GoToPacmanGame();
    }
}