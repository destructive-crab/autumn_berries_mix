using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using mix.code_base;
using mix.code_base.gameplay.systems;

namespace destruct.tweens;

public struct RotationTween
{
    public float DestinationValue;
    public float Speed;
    
    public Action OnCompleted;
    
    public Func<float, RotationTween, float> F;

    public RotationTween(float speed, float destinationRotation)
    {
        Speed = speed;
        DestinationValue = destinationRotation;

        F = (a, t) => a + Game.DeltaTime * t.Speed;
    }
    
    public RotationTween(float speed, float destinationRotation, Func<float, RotationTween, float> f)
    {
        Speed = speed;
        DestinationValue = destinationRotation;

        F = f;
    }
}

public sealed class RotationTweenSystem : IGameLogicSystem
{
    private IFilter filter;
    private ECSWorld world;

    public string DebugName() => nameof(PositionTweenSystem);

    [Inject]
    private void Inject(ECSWorld world)
    {
        this.world = world;
        filter = world.Filter()
            .In<Rotation>()
            .In<RotationTween>()
            .Evaluate();
    }
    
    public void Process()
    {
        if (filter == null || filter.Entities == null) return;

        if (filter.Entities.Count() != 0) LGR.PM(filter.Entities.Count());
        
        foreach (int entity in filter.Entities)
        {
            ref RotationTween tween    = ref world.GetFrom<RotationTween>(entity);
            ref Rotation      rotation = ref world.GetFrom<Rotation>(entity);
                
            rotation.Angle = UTL.MoveTowards(rotation.Angle, tween.DestinationValue, Game.DeltaTime*tween.Speed);

            if (Math.Abs(rotation.Angle-tween.DestinationValue) < 0.1f)
            {
                world.Pool<RotationTween>().Remove(entity);
                tween.OnCompleted?.Invoke();
            }
        }
    }
}