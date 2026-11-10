using System.Numerics;

namespace destruct.tweens;

public struct PositionTween
{
    public Vector2 Destination;
    public float Speed;
    public Action Completed;

    public PositionTween(Vector2 destination, float speed, Action completed)
    {
        Destination = destination;
        Speed = speed;
        Completed = completed;
    }
}