using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;

namespace mix.gameplay.overlay.tweens;

public struct SinScaleTween
{
    public float CurrentSinAngle;
    public float Speed;

    public SinScaleTween(float speed)
    {
        Speed = speed;
    }
}

public sealed class SinScaleTweenSystem : IPreRendererSystem
{
    public string DebugName() => nameof(SinScaleTweenSystem);

    private IFilter filter;
    private ECSWorld world;

    [Inject]
    private void Inject(ECSWorld world)
    {
        this.world = world;
        filter = world.Filter()
            .In<SinScaleTween>()
            .In<Scale>()
            .Evaluate();
    }

    public void PreRenderer()
    {
        foreach (int entity in filter.Entities)
        {
            world.GetFrom<Scale>(entity).Y = 1 + MathF.Sin(world.GetFrom<SinScaleTween>(entity).CurrentSinAngle)/20;
            ref SinScaleTween tween = ref world.GetFrom<SinScaleTween>(entity);
            tween.CurrentSinAngle += Game.DeltaTime*tween.Speed;
            
            if (world.GetFrom<SinScaleTween>(entity).CurrentSinAngle >= 360)
                world.GetFrom<SinScaleTween>(entity).CurrentSinAngle = 0;
        }
    }
}
