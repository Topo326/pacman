namespace PacmanGame.Services;

/// <summary>
/// Define la interfaz para los servicios de sonido del juego.
/// </summary>
public interface ISoundService
{
    /// <summary>
    /// Reproduce el sonido de inicio del juego.
    /// </summary>
    void PlayBeginning();

    /// <summary>
    /// Reproduce el sonido de Pacman comiendo un punto.
    /// </summary>
    void PlayChomp();

    /// <summary>
    /// Reproduce el sonido de muerte de Pacman.
    /// </summary>
    void PlayDeath();

    /// <summary>
    /// Reproduce el sonido de Pacman comiendo un fantasma.
    /// </summary>
    void PlayEatGhost();
}
