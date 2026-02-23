using System;
using System.Diagnostics;
using System.IO;

namespace PacmanGame.Services;

/// <summary>
/// Implementación asíncrona dedicada y construida para la reproducción de sonidos nativos para el kernel o sistema Linux usando el comando aplay de ALSA sin entorpecer la latencia del Engine Gráfico.
/// </summary>
public class LinuxSoundService : ISoundService
{
    private const string AssetPath = "Assets";

    /// <summary>
    /// Estado global de silenciamiento para todos los sonidos de la aplicación.
    /// </summary>
    public static bool GlobalMute { get; set; } = false;

    public bool IsMuted
    {
        get => GlobalMute;
        set => GlobalMute = value;
    }

    public void PlayBeginning() => PlaySound("pacman_beginning.wav");
    public void PlayChomp() => PlaySound("pacman_chomp.wav");
    public void PlayDeath() => PlaySound("pacman_death.wav");
    public void PlayEatGhost() => PlaySound("pacman-eatghost.wav");
    public void PlayEatFruit() => PlaySound("pacman_eatfruit.wav");

    /// <summary>
    /// Invoca el intérprete asíncrono y oculto para la reproducción del contenido multimedia para evitar colgar o afectar directamente el Frame-Rate por la operación de I/O.
    /// </summary>
    /// <param name="fileName">Archivo local y extensionado con formato .wav</param>
    private void PlaySound(string fileName)
    {
        if (IsMuted) return;

        try
        {
            // Ubicación base principal cuando el juego ha sido compilado
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AssetPath, fileName);
            
            if (!File.Exists(path))
            {
                // Fallback de retro-búsqueda utilizado habitualmente cuando se corre en modo Depuración (Debug)
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
            Console.WriteLine($"Error reproduciendo el sonido {fileName}: {ex.Message}");
        }
    }
}
