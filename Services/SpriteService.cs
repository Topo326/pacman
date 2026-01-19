using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using System.IO;
using PacmanGame.Models; // Import the correct namespace for GhostType

namespace PacmanGame.Services;

public class SpriteService
{
    private static SpriteService? _instance;
    private readonly Dictionary<string, CroppedBitmap> _sprites;

    private SpriteService()
    {
        _sprites = new Dictionary<string, CroppedBitmap>();
        LoadSprites();
    }

    public static SpriteService Instance => _instance ??= new SpriteService();

    private void LoadSprites()
    {
        var spriteSheetPath = "Assets/Arcade - Pac-Man - Miscellaneous - General Sprites.png";
        var mazeSheetPath = "Assets/Arcade - Pac-Man - Miscellaneous - All Assets_Palettes.png";

        if (!File.Exists(spriteSheetPath) || !File.Exists(mazeSheetPath))
        {
            throw new FileNotFoundException("Sprite sheet not found in Assets folder.");
        }

        var spriteSheet = new Bitmap(spriteSheetPath);
        var mazeSheet = new Bitmap(mazeSheetPath);

        // Example: Load Pac-Man sprite (adjust coordinates as needed)
        _sprites["PacMan"] = new CroppedBitmap(spriteSheet, new Avalonia.PixelRect(0, 0, 16, 16));
        _sprites["GhostRed"] = new CroppedBitmap(spriteSheet, new Avalonia.PixelRect(16, 0, 16, 16));
        _sprites["Wall"] = new CroppedBitmap(mazeSheet, new Avalonia.PixelRect(0, 0, 8, 8));
    }

    public CroppedBitmap GetSprite(string key)
    {
        if (_sprites.TryGetValue(key, out var sprite))
        {
            return sprite;
        }

        throw new KeyNotFoundException($"Sprite with key '{key}' not found.");
    }

    public string GetPlayerSprite(string direction)
    {
        return direction switch
        {
            "up" => "Assets/PacMan_Up.png",
            "down" => "Assets/PacMan_Down.png",
            "left" => "Assets/PacMan_Left.png",
            "right" => "Assets/PacMan_Right.png",
            _ => "Assets/PacMan_Default.png"
        };
    }

    public string GetPlayerSprite(int dx, int dy)
    {
        if (dx == -1) return GetPlayerSprite("left");
        if (dx == 1) return GetPlayerSprite("right");
        if (dy == -1) return GetPlayerSprite("up");
        if (dy == 1) return GetPlayerSprite("down");
        return GetPlayerSprite("default");
    }

    public string GetGhostSprite(GhostType type)
    {
        return type switch
        {
            GhostType.Blinky => "Assets/Ghost_Red.png",
            GhostType.Pinky => "Assets/Ghost_Pink.png",
            GhostType.Inky => "Assets/Ghost_Blue.png",
            GhostType.Clyde => "Assets/Ghost_Orange.png",
            _ => "Assets/Ghost_Default.png"
        };
    }
}