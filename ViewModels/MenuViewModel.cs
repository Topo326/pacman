using CommunityToolkit.Mvvm.Input;

namespace PacmanGame.ViewModels;

/// <summary>
/// ViewModel que gestiona de manera reactiva la lógica y comandos de la vista del Menú Principal.
/// </summary>
public partial class MenuViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;

    /// <summary>
    /// Constructor del ViewModel del Menú.
    /// </summary>
    /// <param name="mainViewModel">Referencia inyectada para poder solicitar navegaciones a otras vistas principales.</param>
    public MenuViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    /// <summary>
    /// Comando enlazado al botón "Start Game" que instruye al contenedor principal iniciar la partida.
    /// </summary>
    [RelayCommand]
    public void StartGame()
    {
        _mainViewModel.StartGame();
    }

    /// <summary>
    /// Comando enlazado al botón "Exit" que finaliza abruptamente el proceso del juego.
    /// </summary>
    [RelayCommand]
    public void Exit()
    {
        System.Environment.Exit(0);
    }

    /// <summary>
    /// Comando enlazado al botón "High Scores" que transita la vista actual a la pantalla de Récords.
    /// </summary>
    [RelayCommand]
    public void ShowHighScores()
    {
        _mainViewModel.ShowHighScores();
    }

    /// <summary>
    /// Propiedad que refleja el estado global del sonido para la UI.
    /// </summary>
    public bool IsMuted => Services.LinuxSoundService.GlobalMute;

    /// <summary>
    /// Alterna el estado global de silenciamiento.
    /// </summary>
    [RelayCommand]
    public void ToggleMute()
    {
        Services.LinuxSoundService.GlobalMute = !Services.LinuxSoundService.GlobalMute;
        OnPropertyChanged(nameof(IsMuted));
    }
}