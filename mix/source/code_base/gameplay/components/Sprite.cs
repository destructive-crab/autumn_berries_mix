using System.Numerics;
using destruct.imgui;
using Raylib_cs;

namespace mix.code_base;

public struct Sprite
{
    public bool Hidden;
    
    public int DrawOrder;
    
    public Vector2 Pivot;
    public Vec2I DrawOffsetInPixels;
    
    public CachedTexture Texture;

    public Rectangle Rect;
    
    public Sprite()
    {
        DrawOrder = 0;
        Texture = default;
    }

    public static Sprite From(string assetPath)
    {
        Sprite s = new();
        s.Texture = Assets.LoadTexture(assetPath);
        s.Rect = new Rectangle(0, 0, s.Texture.Get().Width, s.Texture.Get().Height);
        s.Pivot = new(0.5f, 0.5f);
        s.Hidden = false;
        s.DrawOrder = 0;
        return s;
    }
}