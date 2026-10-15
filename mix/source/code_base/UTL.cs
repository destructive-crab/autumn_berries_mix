using System.Numerics;
using System.Text;
using destruct.Debug;
using destruct.ecs;
using mix.code_base;

namespace destruct;

public static class UTL
{
    public static int   XYToIndex(int x, int y, int w, int h) => y * w + x;
    public static int   V2IToIndex(Vec2I v2i, int w, int h) => XYToIndex(v2i.X, v2i.Y, w, h);
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

    public static Vector2 TranslatePositionInPixelsToUnits(Vector2 pixels)
    {
        return new Vector2(pixels.X / Global.PixelsPerUnit, pixels.Y / Global.PixelsPerUnit);
    }

    public static int CompareSpritesRendererOrder(int a, int b, ECSWorld world)
    {
        Sprite aSprite = world.Pool<Sprite>().Get(a);
        Sprite bSprite = world.Pool<Sprite>().Get(b);

        Position aPosition = world.Pool<Position>().Get(a);
        Position bPosition = world.Pool<Position>().Get(b);
        
        Vector2 aOrigin = new (0, 0);
        Vector2 bOrigin = new(0, 0);
        
        if(world.Pool<Origin>().Has(a)) aOrigin = world.Pool<Origin>().Get(a).ToV2();
        if(world.Pool<Origin>().Has(b)) bOrigin = world.Pool<Origin>().Get(b).ToV2();

        var aY = aPosition.Y * Global.PixelsPerUnit - aOrigin.Y * aSprite.Fragment.Rect.Height;
        var bY = bPosition.Y * Global.PixelsPerUnit - bOrigin.Y * bSprite.Fragment.Rect.Height;

        if (aPosition.ZZZ > bPosition.ZZZ)
            return 1;

        if (aPosition.ZZZ == bPosition.ZZZ)
        {
            if (aY > bY)                   return 1;
            if (Math.Abs(aY - bY) <= 0.01) return 0;

            return -1;
        }

        return -1;
    }

    public static int CompareZOrderInPosition(int a, int b, ECSWorld world)
    {
        Position aPosition = world.Pool<Position>().Get(a);
        Position bPosition = world.Pool<Position>().Get(b);

        if (aPosition.ZZZ > bPosition.ZZZ) return 1;
        if (aPosition.ZZZ < bPosition.ZZZ) return -1;

        return 0;
    }
    
    public static Vector2 UnitsToPixels(in Position position)
    {
        int x = (int)Math.Round(position.X * Global.PixelsPerUnit);
        int y = (int)Math.Round(position.Y * Global.PixelsPerUnit);

        return new(x,y);
    }
    
    public static Vector2 PixelsToUnits(in Position position)
    {
        int x = (int)Math.Round(position.X / Global.PixelsPerUnit);
        int y = (int)Math.Round(position.Y / Global.PixelsPerUnit);

        return new(x,y);
    }

    public static Vector2 GetWorldScale(in Scale scale)
    {
        int x = (int)Math.Round(scale.X * Global.PixelsPerUnit);
        int y = (int)Math.Round(scale.Y * Global.PixelsPerUnit);

        return new(x, y);
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

    public static string ToStringEnumerable<T>(IEnumerable<T> enumerable)
    {
        StringBuilder builder = new("[");
        foreach (object o in enumerable)
        {
            builder.Append(o.ToString() + "; ");
        }
        builder.Append("]");
        return builder.ToString();
    }
}

public enum GameDirection
{
    Left,
    Right,
    Up,
    Down
}