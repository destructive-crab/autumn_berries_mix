using System.Numerics;

namespace mix.code_base;

public struct Position
{
    public float X;
    public float Y;

    public int ZZZ;

    public void Set(float x, float y)
    {
        X = x;
        Y = y;
    }
    
    public void Set(float x, float y, int z)
    {
        Set(x, y);
        ZZZ = z;
    }

    public Vector2 ToV2()
    {
        return new(X, Y);
    }
}