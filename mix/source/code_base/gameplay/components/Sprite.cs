using System.Numerics;
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
}

public struct Sprite
{
    public bool Hidden;
    
    public Vec2I DrawOffsetInPixels;
    
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