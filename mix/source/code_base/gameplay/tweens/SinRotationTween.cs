using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;

namespace mix.gameplay.overlay.tweens;

public struct SinRotationTween
{
    public float CurrentSinAngle;
    
    public int Radius;
    public float Speed;

    public SinRotationTween(int radius, float speed)
    {
        Radius = radius;
        Speed = speed;
    }
}

public sealed class SinRotationTweenSystem : IPreWorldRendererSystem
{
    public string DebugName() => nameof(SinRotationTweenSystem);

    private ECSWorld world;
    private IFilter filter;
    
    [Inject]
    private void Inject(ECSWorld world)
    {
        this.world = world;
        filter = world.Filter()
            .In<SinRotationTween>()
            .In<Rotation>()
            .Evaluate();
    }
    
    public void PreWorldRenderer()
    {
        foreach (int entity in filter.Entities)
        {
            ref SinRotationTween tween = ref world.GetFrom<SinRotationTween>(entity);

            world.GetFrom<Rotation>(entity).Angle = tween.Radius * MathF.Sin(tween.CurrentSinAngle);
            
            tween.CurrentSinAngle += Game.DeltaTime*tween.Speed;
            
            if (world.GetFrom<SinScaleTween>(entity).CurrentSinAngle >= 360)
                world.GetFrom<SinScaleTween>(entity).CurrentSinAngle = 0;
        }
    }
}