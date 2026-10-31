using destruct.ecs;
using mix.code_base.gameplay.units;

namespace mix.code_base.gameplay;

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