using System.Numerics;
using destruct.ecs;

namespace destruct.tweens;

public static class Tweens
{
    public static async Task MoveTween(ECSWorld world, int entity, Vector2 destPosition, int speed)
    {
        bool completed = false;
        world.Pool<PositionTween>().New(entity, (ref PositionTween t) =>
        {
            t.Destination = destPosition;
            t.Speed = speed;
            t.Completed += () =>
            {
                completed = true;
            }; 
        });
        
        while (!completed) await Task.Yield();
    }

    public static void MoveTween(ECSWorld world, int entity, Vector2 destPosition, int speed, Action onCompleted)
    {
        world.Pool<PositionTween>().New(entity, (ref PositionTween t) =>
        {
            t.Destination = destPosition;
            t.Speed = speed;
            t.Completed += onCompleted;
        });
    }
}