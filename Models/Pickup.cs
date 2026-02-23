namespace PacmanGame.Models;

/// <summary>
/// Tipos de frutas / coleccionables especiales disponibles en el juego.
/// </summary>
public enum PickupType
{
    Cherry,
    Strawberry
}

/// <summary>
/// Representa una entidad recolectable (Fruta) que aparece temporalmente en el mapa.
/// Gestiona su posición, tipo, visibilidad y el tiempo que le queda antes de desaparecer.
/// </summary>
public class Pickup
{
    public double X { get; set; }
    public double Y { get; set; }
    public PickupType Type { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Tiempo restante en milisegundos o segundos que la fruta permanecerá en el mapa.
    /// </summary>
    public double TimeLeft { get; set; }

    public Pickup(double x, double y, PickupType type, double durationSeconds)
    {
        X = x;
        Y = y;
        Type = type;
        TimeLeft = durationSeconds;
    }

    /// <summary>
    /// Actualiza el temporizador de vida de la fruta. Si llega a 0 se inactiva.
    /// </summary>
    public void Update(double deltaTime)
    {
        if (!IsActive) return;

        TimeLeft -= deltaTime;
        if (TimeLeft <= 0)
        {
            IsActive = false;
        }
    }
}
