using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PacmanGame.ViewModels;
public partial class EntityViewModel : ViewModelBase
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private Bitmap? _sprite;

    public void LoadSprite(string assetPath)
    {
        var uri = new Uri(assetPath);
        Sprite = new Bitmap(AssetLoader.Open(uri));
    }
    
    public virtual void Move() { }
}