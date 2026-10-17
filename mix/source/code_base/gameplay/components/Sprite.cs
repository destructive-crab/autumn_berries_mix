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
    public float Rotation;
    
    public bool MirroredHorizontal;
    public bool MirroredVertical;

    public TextureFragment Fragment;

    public static Sprite From(string assetPath)
    {
        Sprite s = new();
        
        s.Fragment = TextureFragment.Full(Assets.LoadTexture(assetPath));
        s.Hidden = false;
        return s;
    }
}