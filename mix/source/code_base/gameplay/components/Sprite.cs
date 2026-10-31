using System.Numerics;
using destruct.Debug;
using Raylib_cs;

namespace mix.code_base;

public struct Rotation
{
    public float Angle;

    public Rotation(float angle)
    {
        Angle = angle;
    }
}

public struct Scale
{
    public float X;
    public float Y;

    public Scale(float x, float y)
    {
        X = x;
        Y = y;
    }

    public void Set(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vector2 ToV2()
    {
        return new(X, Y);
    }

    public static Scale One()
    {
        return new Scale(1, 1);
    }
}

public struct Origin
{
    public float X;
    public float Y;

    public Origin(float x, float y)
    {
        X = x;
        Y = y;
    }

    public void Set(float x, float y)
    {
        X = x;
        Y = y;
    }

    public Vector2 ToV2()
    {
        return new(X, Y);
    }

    public static Origin Center()
    {
        return new(0.5f, 0.5f);
    }
    
    public static Origin CenterBottom()
    {
        return new(0.5f, 1f);
    }
}

public struct DrawOffset
{
    public Vec2I OffsetInPixels;

    public DrawOffset(int x, int y)
    {
        OffsetInPixels = new(x, y);
    }

    public static DrawOffset FromUnits(float x, float y)
    {
        return new DrawOffset((int)MathF.Ceiling(Global.PixelsPerUnit * x), (int)MathF.Ceiling(Global.PixelsPerUnit * y));
    }
}

public struct Sprite
{
    public bool Hidden;
    
    public bool MirroredHorizontal;
    public bool MirroredVertical;

    public TextureFragment Fragment;

    public static Sprite From(string assetPath)
    {
        Sprite s = new();
        
        s.Fragment = TextureFragment.Full(Assets.Textures.Load(assetPath));
        s.Hidden = false;
        return s;
    }
}