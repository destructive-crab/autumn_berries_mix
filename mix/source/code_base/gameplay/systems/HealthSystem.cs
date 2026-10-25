using destruct.DI;
using destruct.ecs;

namespace mix.code_base.gameplay.systems;

public sealed class HealthSystem : IGameLogicSystem
{
    public string DebugName() => nameof(HealthSystem);

    private IFilter healthFilter;
    private ECSWorld world;

    private readonly Dictionary<int, int> healthLastFrame = new();
    
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
                //if (AnimatorUTL.AnimatorHasState(world, entity, "death"))
                //{
                //    PlayDeathAnimationAndDelete(entity);
                //}
                //else
                //{
                //    world.Delete(entity);
                //}
            }
            
            //healthLastFrame.Add(entity, world.GetFrom<Health>(entity).Current);
        }
    }

    private void PlayDeathAnimationAndDelete(int entity)
    {
        world.Delete(entity);
    }
}