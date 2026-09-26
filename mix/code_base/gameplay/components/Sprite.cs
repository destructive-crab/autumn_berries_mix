using System.Numerics;
using Raylib_cs;

namespace mix.code_base;

public struct Sprite
{
    public bool Hidden;
    
    public int DrawOrder;
    
    public Vector2 Pivot;
    public Vec2I DrawOffsetInPixels;
    
    public Texture2D Texture;

    public Sprite()
    {
        DrawOrder = 0;
        Texture = default;
    }
}