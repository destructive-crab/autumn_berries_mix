using System.Numerics;
using destruct.Debug;

namespace mix.code_base;

public struct Position
{
    public float X;
    public float Y;

    public int ZZZ;

    public Position(float x, float y, int zzz)
    {
        X = x;
        Y = y;
        ZZZ = zzz;
    }

    public void Set(in Position position)
    {
        X = position.X;
        Y = position.Y;
    }

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
    
    public Vec2I ToRoundedV2I()
    {
        return new((int)Math.Round(X), (int)Math.Round(Y));
    }
    
    public bool MoveTowards(Vector2 target, float maxDistanceDelta)
    {
        Vector2 current = new(X, Y);
        Vector2 toVector = target - current;
        
        float dist = toVector.Length();

        if (dist <= maxDistanceDelta || dist == 0f)
        {
            return true;
        }

        current += (toVector / dist) * maxDistanceDelta;
        X = current.X;
        Y = current.Y;

        return false;
    }
}