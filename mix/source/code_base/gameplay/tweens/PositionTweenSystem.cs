using System.Numerics;
using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;

namespace mix.gameplay.overlay.tweens;

public sealed class PositionTweenSystem : ISystem, IWorldRendererSystem
{
    private IFilter filter;
    private ECSWorld world;

    public string DebugName() => nameof(PositionTweenSystem);

    [Inject]
    private void Inject(ECSWorld world)
    {
        this.world = world;
        filter = world.Filter()
            .In<Position>()
            .In<PositionTween>()
            .Evaluate();
    }

    public void RenderWorld()
    {
        if (filter == null || filter.Entities == null) return;
        foreach (int entity in filter.Entities)
        {
            var tween = world.GetFrom<PositionTween>(entity);
            if (world.GetFrom<Position>(entity).MoveTowards(tween.Destination, tween.Speed * Game.DeltaTime))
            {
                world.Pool<PositionTween>().Remove(entity);
                tween.Completed?.Invoke();
            }
        }
    }
}