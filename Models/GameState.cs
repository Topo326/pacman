using System;

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
    
    // Modos cíclicos
    public bool IsScatterMode { get; set; } = false;
    public double ModeTimeLeft { get; set; } = GameConstants.ChaseDurationSeconds;
    
    // Timer para animación de muerte
    public double DeathTimer { get; set; }
    
    // Pickups Timers
    public double CherrySpawnTimer { get; set; } = 15.0;     // 15 segundos para re-aparecer
    public double StrawberrySpawnTimer { get; set; } = 30.0; // 30 segundos para re-aparecer
    
    // Evento para notificar inversión de dirección
    public event Action? OnModeChanged;

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
        IsScatterMode = false;
        ModeTimeLeft = GameConstants.ChaseDurationSeconds;
        CherrySpawnTimer = 15.0;
        StrawberrySpawnTimer = 30.0;
    }

    /// <summary>
    /// Activa el modo asustado para los fantasmas.
    /// </summary>
    public void ActivateFrightenedMode()
    {
        IsFrightenedMode = true;
        FrightenedTimeLeft = GameConstants.PowerPillDurationSeconds;
        OnModeChanged?.Invoke();
    }

    /// <summary>
    /// Actualiza los temporizadores del estado del juego.
    /// </summary>
    public void Update(double deltaTime)
    {
        ElapsedSeconds += deltaTime;
        
        // Descuenta el tiempo hasta spawnear los pickups
        if (CherrySpawnTimer > 0) CherrySpawnTimer -= deltaTime;
        if (StrawberrySpawnTimer > 0) StrawberrySpawnTimer -= deltaTime;
        
        if (IsFrightenedMode)
        {
            FrightenedTimeLeft -= deltaTime;
            if (FrightenedTimeLeft <= 0)
            {
                IsFrightenedMode = false;
                FrightenedTimeLeft = 0;
                // No invertimos al final del modo asustado
            }
        }
        else
        {
            // Solo avanza el timer cíclico si NO estamos asustados
            ModeTimeLeft -= deltaTime;
            if (ModeTimeLeft <= 0)
            {
                IsScatterMode = !IsScatterMode;
                ModeTimeLeft = IsScatterMode ? GameConstants.ScatterDurationSeconds : GameConstants.ChaseDurationSeconds;
                OnModeChanged?.Invoke();
            }
        }
    }
}
