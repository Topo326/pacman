namespace PacmanGame.Models;

/// <summary>
/// Contiene las constantes globales del juego Pacman.
/// </summary>
public static class GameConstants
{
    public const int TileSize = 26;
    public const int PacmanSpeed = 3;
    public const int GhostSpeed = 2;
    public const int GhostFrightenedSpeed = 1;
    public const int PowerPillDurationSeconds = 10;
    public const int DotScore = 10;
    public const int PowerPillScore = 50;
    public const int GhostEatScore = 200;
    
    // Timer para IA de fantasmas
    public const int ScatterDurationSeconds = 7;
    public const int ChaseDurationSeconds = 20;
}
