using System.Numerics;
using Raylib_cs;

namespace mix.code_base;

public struct Rotation
{
    public float Angle;
}

public struct Scale
{
    public float X;
    public float Y;

    public void Set(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public struct Origin
{
    public float X;
    public float Y;

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
    
    public CachedTexture Texture;

    public Rectangle Rect;
    
    public Sprite()
    {
        Texture = default;
    }

    public static Sprite From(string assetPath)
    {
        Sprite s = new();
        s.Texture = Assets.LoadTexture(assetPath);
        s.Rect = new Rectangle(0, 0, s.Texture.Get().Width, s.Texture.Get().Height);
        s.Hidden = false;
        return s;
    }
}