using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PacmanGame.Models;

public class ScoreBoard
{
    private const string FileName = "top_scores.json";
    public List<int> TopScores { get; private set; } = new List<int>();

    public ScoreBoard()
    {
        Load();
    }

    public void AddScore(int score)
    {
        TopScores.Add(score);
        TopScores = TopScores.OrderByDescending(s => s).Take(5).ToList();
        Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(FileName)) return;
            var json = File.ReadAllText(FileName);
            TopScores = JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch
        {
            TopScores = new List<int>();
        }
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(TopScores);
            File.WriteAllText(FileName, json);
        }
        catch { }
    }
}
