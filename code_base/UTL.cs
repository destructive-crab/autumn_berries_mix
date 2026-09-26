using System;
using System.Net.Sockets;
using Godot;

namespace destruct.Debug;

public static class UTL
{
    public static int XYToIndex(int x, int y, int w, int h) => y * w + x;
    public static int V2IToIndex(Vector2I v2i, int w, int h) => XYToIndex(v2i.X, v2i.Y, w, h);
    public static Vector2I IndexToV2I(int index, int w, int h) => new Vector2I(index % h, index / h);

    public static Vector2I[] GetAllOffsets()
    {
        return
        [
            new Vector2I( 0,  1),
            new Vector2I( 1,  0),
            new Vector2I( 0, -1),
            new Vector2I(-1,  0),
            
            new Vector2I( 1,  1),
            new Vector2I(-1, -1),
            new Vector2I( 1, -1),
            new Vector2I(-1,  1),
        ];
    }

    public static Vector2I[] GetOnlyStraightOffsets()
    {
        return
        [
            new Vector2I( 0,  1),
            new Vector2I( 1,  0),
            new Vector2I( 0, -1),
            new Vector2I(-1,  0),
        ];
    }
    
    public static GameDirection OppositeTo(GameDirection dir)
    {
        switch (dir)
        {
            case GameDirection.Left:  return GameDirection.Right;
            case GameDirection.Right: return GameDirection.Left;
            case GameDirection.Up:    return GameDirection.Down;
            case GameDirection.Down:  return GameDirection.Up;
        }

        throw new Exception("Invalid direction was provided; opposite dir not found");
    }

    public static Vector2I V2IFromDir(GameDirection dir)
    {
        switch (dir)
        {
            case GameDirection.Left:  return Vector2I.Left; 
            case GameDirection.Right: return Vector2I.Right;
            case GameDirection.Up:    return Vector2I.Up;   
            case GameDirection.Down:  return Vector2I.Down; 
        }
        
        throw new Exception("Invalid direction was provided; opposite dir not found");
    }

    public static Vector2I OtherSocketTileOffset(GameDirection dir)
    {
        switch (dir)
        {
            case GameDirection.Left:
            case GameDirection.Right:  return Vector2I.Down;
            
            case GameDirection.Up:
            case GameDirection.Down:    return Vector2I.Right;
        }
        
        throw new Exception("Invalid direction was provided; opposite dir not found");
    }
}

public enum GameDirection
{
    Left,
    Right,
    Up,
    Down
}