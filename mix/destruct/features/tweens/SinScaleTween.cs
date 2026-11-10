using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using destruct;
using destruct.renderer;

namespace destruct.tweens;

public struct SinScaleTween : ISinAngleComponent
{
    public float Speed { get; set; }
    public float CurrentSinAngle { get; set; }

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
            
            SinAngleUpdateUTL.UpdateAngle(ref world.GetFrom<SinScaleTween>(entity));
        }
    }
}
