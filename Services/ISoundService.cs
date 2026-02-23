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

    /// <summary>
    /// Reproduce el sonido de Pacman comiendo una fruta (pickup).
    /// </summary>
    void PlayEatFruit();

    /// <summary>
    /// Obtiene o establece un valor que indica si el sistema de sonido está silenciado.
    /// </summary>
    bool IsMuted { get; set; }
}
