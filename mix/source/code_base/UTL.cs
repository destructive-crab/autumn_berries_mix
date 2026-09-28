namespace destruct;

public static class UTL
{
    public static int XYToIndex(int x, int y, int w, int h) => y * w + x;
    public static int V2IToIndex(Vec2I v2i, int w, int h) => XYToIndex(v2i.X, v2i.Y, w, h);
    public static Vec2I IndexToV2I(int index, int w, int h) => new Vec2I(index % h, index / h);

    public static Vec2I[] GetAllOffsets()
    {
        return
        [
            new Vec2I( 0,  1),
            new Vec2I( 1,  0),
            new Vec2I( 0, -1),
            new Vec2I(-1,  0),
            
            new Vec2I( 1,  1),
            new Vec2I(-1, -1),
            new Vec2I( 1, -1),
            new Vec2I(-1,  1),
        ];
    }

    public static Vec2I[] GetOnlyStraightOffsets()
    {
        return
        [
            new Vec2I( 0,  1),
            new Vec2I( 1,  0),
            new Vec2I( 0, -1),
            new Vec2I(-1,  0),
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

    public static Vec2I V2IFromDir(GameDirection dir)
    {
        switch (dir)
        {
            case GameDirection.Left:  return Vec2I.Left; 
            case GameDirection.Right: return Vec2I.Right;
            case GameDirection.Up:    return Vec2I.Up;   
            case GameDirection.Down:  return Vec2I.Down; 
        }
        
        throw new Exception("Invalid direction was provided; opposite dir not found");
    }

    public static Vec2I OtherSocketTileOffset(GameDirection dir)
    {
        switch (dir)
        {
            case GameDirection.Left:
            case GameDirection.Right:  return Vec2I.Down;
            
            case GameDirection.Up:
            case GameDirection.Down:    return Vec2I.Right;
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