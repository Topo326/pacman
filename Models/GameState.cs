namespace PacmanGame.Models;

/// <summary>
/// Mantiene el estado actual del juego, incluyendo puntuación, vidas y modos de juego.
/// </summary>
public class GameState
{
    public int Score { get; set; }
    public int HighScore { get; set; }
    public int Lives { get; set; } = 3;
    public double ElapsedSeconds { get; set; }
    public bool IsGameOver => Lives <= 0;
    
    // Estado de los fantasmas
    public bool IsFrightenedMode { get; set; }
    public double FrightenedTimeLeft { get; set; }

    /// <summary>
    /// Incrementa la puntuación y actualiza el récord si es necesario.
    /// </summary>
    public void AddScore(int points)
    {
        Score += points;
        if (Score > HighScore)
        {
            HighScore = Score;
        }
    }

    public void LoseLife() => Lives--;

    /// <summary>
    /// Reinicia el estado del juego a los valores iniciales.
    /// </summary>
    public void Reset()
    {
        Score = 0;
        Lives = 3;
        ElapsedSeconds = 0;
        IsFrightenedMode = false;
        FrightenedTimeLeft = 0;
    }

    /// <summary>
    /// Activa el modo asustado para los fantasmas.
    /// </summary>
    public void ActivateFrightenedMode()
    {
        IsFrightenedMode = true;
        FrightenedTimeLeft = GameConstants.PowerPillDurationSeconds;
    }

    /// <summary>
    /// Actualiza los temporizadores del estado del juego.
    /// </summary>
    public void Update(double deltaTime)
    {
        ElapsedSeconds += deltaTime;
        if (IsFrightenedMode)
        {
            FrightenedTimeLeft -= deltaTime;
            if (FrightenedTimeLeft <= 0)
            {
                IsFrightenedMode = false;
                FrightenedTimeLeft = 0;
            }
        }
    }
}
