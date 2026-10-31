using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;

namespace mix.gameplay.overlay.tweens;

public struct SinYPositionTween
{
    public float CurrentSinAngle;
    public float Speed;

    public int Radius;

    public SinYPositionTween(float speed)
    {
        Speed = speed;
    }
}

public sealed class SinYPositionTweenSystem : IPreRendererSystem
{
    public string DebugName() => nameof(SinScaleTweenSystem);

    private IFilter filter;
    private ECSWorld world;

    [Inject]
    private void Inject(ECSWorld world)
    {
        this.world = world;
        filter = world.Filter()
            .In<SinYPositionTween>()
            .In<Scale>()
            .Evaluate();
    }

    public void PreRenderer()
    {
        foreach (int entity in filter.Entities)
        {
            world.GetFrom<Scale>(entity).Y = 1 + MathF.Sin(world.GetFrom<SinYPositionTween>(entity).CurrentSinAngle)/20;
            ref SinYPositionTween tween = ref world.GetFrom<SinYPositionTween>(entity);
            tween.CurrentSinAngle += Game.DeltaTime*tween.Speed;
            
            if (world.GetFrom<SinYPositionTween>(entity).CurrentSinAngle >= 360)
                world.GetFrom<SinYPositionTween>(entity).CurrentSinAngle = 0;
        }
    }
}
