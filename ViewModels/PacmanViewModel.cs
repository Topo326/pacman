using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PacmanGame.Models;

namespace PacmanGame.ViewModels;

public partial class PacmanViewModel : ViewModelBase
{
    private const int BLOCK_SIZE = 20;
    
    [ObservableProperty] private double x;
    [ObservableProperty] private double y;
    [ObservableProperty] private int width;
    [ObservableProperty] private int height;
    [ObservableProperty] private double speed;
    [ObservableProperty] private Direction direction;
    [ObservableProperty] private Direction nextDirection;
    [ObservableProperty] private int currentFrame;
    [ObservableProperty] private int frameCount;
    
    public PacmanViewModel(double x, double y, int width, int height, double speed)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Speed = speed;
        Direction = Direction.Right;
        NextDirection = Direction.Right;
        FrameCount = 7;
        CurrentFrame = 1;
    }
    
    public void MoveProcess(int[,] gameBoard)
    {
        ChangeDirectionIfPossible(gameBoard);
        MoveForwards();
        
        if (CheckCollisions(gameBoard))
        {
            MoveBackwards();
        }
    }
    
    private void ChangeDirectionIfPossible(int[,] gameBoard)
    {
        if (Direction == NextDirection) return;
        
        Direction tempDirection = Direction;
        Direction = NextDirection;
        MoveForwards();
        
        if (CheckCollisions(gameBoard))
        {
            MoveBackwards();
            Direction = tempDirection;
        }
        else
        {
            MoveBackwards();
        }
    }
    
    private void MoveForwards()
    {
        switch (Direction)
        {
            case Direction.Right: X += Speed; break;
            case Direction.Up: Y -= Speed; break;
            case Direction.Left: X -= Speed; break;
            case Direction.Bottom: Y += Speed; break;
        }
    }
    
    private void MoveBackwards()
    {
        switch (Direction)
        {
            case Direction.Right: X -= Speed; break;
            case Direction.Up: Y += Speed; break;
            case Direction.Left: X += Speed; break;
            case Direction.Bottom: Y -= Speed; break;
        }
    }
    
    // Verifica colisiones usando offset 0.9999 para detectar bordes de tiles
    private bool CheckCollisions(int[,] gameBoard)
    {
        int rows = gameBoard.GetLength(0);
        int cols = gameBoard.GetLength(1);
        
        int row1 = (int)(Y / BLOCK_SIZE);
        int col1 = (int)(X / BLOCK_SIZE);
        int row2 = (int)((Y / BLOCK_SIZE) + 0.9999);
        int col2 = (int)((X / BLOCK_SIZE) + 0.9999);
        
        if (row1 < 0 || row1 >= rows || col1 < 0 || col1 >= cols ||
            row2 < 0 || row2 >= rows || col2 < 0 || col2 >= cols)
            return true;
        
        return gameBoard[row1, col1] == 1 || gameBoard[row2, col1] == 1 ||
               gameBoard[row1, col2] == 1 || gameBoard[row2, col2] == 1;
    }
    
    public int GetMapX() => (int)(X / BLOCK_SIZE);
    public int GetMapY() => (int)(Y / BLOCK_SIZE);
    public int GetMapXRightSide() => (int)((X * 0.99 + BLOCK_SIZE) / BLOCK_SIZE);
    public int GetMapYRightSide() => (int)((Y * 0.99 + BLOCK_SIZE) / BLOCK_SIZE);
    
    [RelayCommand]
    public void ChangeAnimation()
    {
        CurrentFrame = CurrentFrame == FrameCount ? 1 : CurrentFrame + 1;
    }
    
    [RelayCommand]
    public void SetNextDirection(Direction newDirection)
    {
        NextDirection = newDirection;
    }
}
