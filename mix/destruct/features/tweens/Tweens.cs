using System.Numerics;
using destruct.Debug;
using destruct.ecs;
using Raylib_cs;

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
    
    public static void PingPongColorTween(ECSWorld world, int entity, Color destination, float speed)
    {
        if (!world.HasIn<RendererColor>(entity))
        {
            LGR.PERR("Cannot add color tween to entity without color component");
            return;
        }
        
        world.Pool<ColorTween>().New(entity, (ref ColorTween color) =>
        {
            color.Destination = destination;
            color.Speed       = speed;
            color.Initial     = world.GetFrom<RendererColor>(entity).Value;
            color.PingPong    = true;
        });
    }
}