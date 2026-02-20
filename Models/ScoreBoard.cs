using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PacmanGame.Models;

/// <summary>
/// Gestiona la persistencia de las puntuaciones más altas.
/// </summary>
public class ScoreBoard
{
    private const string FileName = "top_scores.json";
    
    /// <summary>
    /// Lista de las 5 puntuaciones más altas.
    /// </summary>
    public List<int> TopScores { get; private set; } = new List<int>();

    public ScoreBoard()
    {
        Load();
    }

    /// <summary>
    /// Añade una nueva puntuación y mantiene solo las 5 mejores.
    /// </summary>
    public void AddScore(int score)
    {
        if (score <= 0) return;
        
        TopScores.Add(score);
        TopScores = TopScores.OrderByDescending(s => s).Take(5).ToList();
        Save();
    }

    /// <summary>
    /// Carga las puntuaciones desde el archivo JSON.
    /// </summary>
    private void Load()
    {
        try
        {
            if (!File.Exists(FileName))
            {
                TopScores = new List<int>();
                return;
            }
            
            var json = File.ReadAllText(FileName);
            TopScores = JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch
        {
            TopScores = new List<int>();
        }
    }

    /// <summary>
    /// Guarda las puntuaciones en el archivo JSON.
    /// </summary>
    private void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(TopScores, options);
            File.WriteAllText(FileName, json);
        }
        catch { }
    }
}
