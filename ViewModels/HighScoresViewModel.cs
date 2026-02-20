using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PacmanGame.Models;

namespace PacmanGame.ViewModels;

public partial class HighScoresViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;
    private readonly ScoreBoard _scoreBoard;

    public ObservableCollection<int> TopScores { get; }

    public HighScoresViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _scoreBoard = new ScoreBoard();
        TopScores = new ObservableCollection<int>(_scoreBoard.TopScores);
    }

    [RelayCommand]
    public void GoBack()
    {
        _mainViewModel.GoToMenu();
    }
}
