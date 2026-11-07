using destruct.ecs;
using destruct.gameplay.units;

namespace destruct.gameplay;

public sealed class GameplayFilters
{
    public readonly IFilter EnemyUnits;
    public readonly IFilter PlayerUnits;

    public GameplayFilters(ECSWorld world)
    {
        EnemyUnits  = world.Filter().In<EnemyUnit>().Evaluate();
        PlayerUnits = world.Filter().In<PlayerUnit>().Evaluate();
    }
}