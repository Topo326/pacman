namespace PacmanGame.Models;

public class GameState
{
    public int Score { get; set; }
    public int HighScore { get; set; }
    public int Lives { get; set; } = 3;
    public double ElapsedSeconds { get; set; }
    public bool IsGameOver => Lives <= 0;
    
    public void AddScore(int points)
    {
        Score += points;
        if (Score > HighScore)
        {
            HighScore = Score;
        }
    }
    public void LoseLife() => Lives--;
    public void Reset()
    {
        Score = 0;
        Lives = 3;
        ElapsedSeconds = 0;
    }
}
