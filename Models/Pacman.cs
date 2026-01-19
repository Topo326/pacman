namespace PacmanGame.Models;

public class Pacman
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Speed { get; set; }
    public int Direction { get; set; }
    public int NextDirection { get; set; }
    public int FrameCount { get; set; }
    public int CurrentFrame { get; set; }

    public Pacman(int x, int y, int width, int height, int speed)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Speed = speed;
        Direction = 4; // Default direction
        NextDirection = 4;
        FrameCount = 7;
        CurrentFrame = 1;
    }

    public void MoveProcess()
    {
        ChangeDirectionIfPossible();
        MoveForwards();
        if (CheckCollisions())
        {
            MoveBackwards();
        }
    }

    public void Eat(int[,] map, ref int score)
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] == 2 && GetMapX() == j && GetMapY() == i)
                {
                    map[i, j] = 3;
                    score++;
                }
            }
        }
    }

    private void MoveBackwards()
    {
        switch (Direction)
        {
            case 4: // Right
                X -= Speed;
                break;
            case 3: // Up
                Y += Speed;
                break;
            case 2: // Left
                X += Speed;
                break;
            case 1: // Down
                Y -= Speed;
                break;
        }
    }

    private void ChangeDirectionIfPossible()
    {
        // Implement logic to change direction if possible
    }

    private void MoveForwards()
    {
        // Implement logic to move Pacman forward
    }

    private bool CheckCollisions()
    {
        // Implement collision detection logic
        return false;
    }

    private int GetMapX()
    {
        return X / Width;
    }

    private int GetMapY()
    {
        return Y / Height;
    }
}