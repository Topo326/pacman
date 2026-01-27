using System;
using System.Diagnostics;
using System.IO;

namespace PacmanGame.Services;

public class LinuxSoundService : ISoundService
{
    private const string AssetPath = "Assets"; // Adjust if needed

    public void PlayBeginning() => PlaySound("pacman_beginning.wav");
    public void PlayChomp() => PlaySound("pacman_chomp.wav");
    public void PlayDeath() => PlaySound("pacman_death.wav");

    private void PlaySound(string fileName)
    {
        try
        {
            // Assuming Assets are copied to output directory or accessible relative to App
            // We might need to find the absolute path or relative to binary
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AssetPath, fileName);
            
            if (!File.Exists(path))
            {
                // Fallback: try looking in project structure if running from IDE (Debug)
                // This is a hack for development
                string debugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Assets", fileName);
                if (File.Exists(debugPath)) path = debugPath;
            }

            if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "aplay",
                    Arguments = $"-q \"{path}\"", // -q for quiet
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            else
            {
                Console.WriteLine($"Sound file not found: {path}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing sound {fileName}: {ex.Message}");
        }
    }
}
