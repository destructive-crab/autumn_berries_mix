using System.Numerics;

namespace destruct.imgui;

public struct Rect
{
    public int X;
    public int Y;
    public int W;
    public int H;

    public Rect(int x, int y, int w, int h)
    {
        X = x;
        Y = y;
        W = w;
        H = h;
    }

    public Rect(int x, int y, Vector2 size)
    {
        X = x;
        Y = y;
        W = (int)size.X;
        H = (int)size.Y;
    }
    
    public Rect(Vector2 p, Vector2 size)
    {
        X = (int)p.X;
        Y = (int)p.Y;
        W = (int)size.X;
        H = (int)size.Y;
    }

    public override string ToString()
    {
        return $"{X} {Y} {W} {H}";
    }
}