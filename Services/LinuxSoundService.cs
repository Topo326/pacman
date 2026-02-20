using System;
using System.Diagnostics;
using System.IO;

namespace PacmanGame.Services;

public class LinuxSoundService : ISoundService
{
    private const string AssetPath = "Assets";

    public void PlayBeginning() => PlaySound("pacman_beginning.wav");
    public void PlayChomp() => PlaySound("pacman_chomp.wav");
    public void PlayDeath() => PlaySound("pacman_death.wav");
    public void PlayEatGhost() => PlaySound("pacman-eatghost.wav");

    private void PlaySound(string fileName)
    {
        try
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AssetPath, fileName);
            
            if (!File.Exists(path))
            {
        
                string debugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Assets", fileName);
                if (File.Exists(debugPath)) path = debugPath;
            }

            if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "aplay",
                    Arguments = $"-q \"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing sound {fileName}: {ex.Message}");
        }
    }
}
