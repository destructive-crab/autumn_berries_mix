using destruct.DI;
using destruct.ecs;
using mix.gameplay.systems;

namespace mix.code_base.gameplay.systems;

public sealed class HealthSystem : IGameLogicSystem
{
    public string DebugName() => nameof(HealthSystem);

    private IFilter healthFilter;
    private ECSWorld world;

    [Inject]
    private void Inject(ECSWorld world)
    {
        this.world = world;
        healthFilter = world.Filter().In<Health>().Evaluate();
    }
    
    public void Process()
    {
        foreach (int entity in healthFilter.Entities)
        {
            if (world.GetFrom<Health>(entity).Current <= 0)
            {
                EventUtils.TryPushEventToEntity(world, entity, new Hit());
                    
                //if (AnimatorUTL.AnimatorHasState(world, entity, "death"))
                //{
                //    PlayDeathAnimationAndDelete(entity);
                //}
                //else
                //{
                //    world.Delete(entity);
                //}
            }
        }
    }

    private void PlayDeathAnimationAndDelete(int entity)
    {
        world.Delete(entity);
    }
}