using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.gameplay.systems;
using Raylib_cs;

namespace destruct.tweens;

public struct ColorTween
{
    public Color Initial;
    public Color Destination;

    public float Speed;
    public bool PingPong;

    public ColorTween(Color destination, float speed)
    {
        Destination = destination;
        Speed = speed;
    }
}

public sealed class ColorTweenSystem : IGameLogicSystem
{
    public string DebugName() => nameof(ColorTweenSystem);

    private IFilter filter;
    private ECSWorld world;

    [Inject]
    private void Inject(ECSWorld world)
    {
        this.world = world;

        filter = world.Filter()
            .In<RendererColor>()
            .In<ColorTween>();
        
    }

    public void Process()
    {
        foreach (int entity in filter.Entities)
        {
            ref RendererColor entityColor = ref world.GetFrom<RendererColor>(entity);
            ref ColorTween tween = ref world.GetFrom<ColorTween>(entity);

            byte r = (byte)UTL.MoveTowards(entityColor.Value.R, tween.Destination.R, Game.DeltaTime * tween.Speed);
            byte g = (byte)UTL.MoveTowards(entityColor.Value.G, tween.Destination.G, Game.DeltaTime * tween.Speed);
            byte b = (byte)UTL.MoveTowards(entityColor.Value.B, tween.Destination.B, Game.DeltaTime * tween.Speed);
            byte a = (byte)UTL.MoveTowards(entityColor.Value.A, tween.Destination.A, Game.DeltaTime * tween.Speed);
            
            entityColor.Value = new(r, g, b, a);
            
            if (entityColor.Value == tween.Destination)
            {
                LGR.PM("ENDED");
                if (tween.PingPong)
                {
                    tween.Destination = tween.Initial;
                    tween.PingPong = false;
                    LGR.PM($"DOING PING PONG for {tween.Initial} {entityColor.Value} {tween.Destination}");
                }
                else
                {
                    world.RemoveFrom<ColorTween>(entity);
                    LGR.PM($"DeLETEd");
                }
            }
        }
    }
}