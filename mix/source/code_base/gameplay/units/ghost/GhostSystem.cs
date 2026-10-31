using autumn_berries_mix;
using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using mix.code_base.gameplay.systems;

namespace mix.code_base.gameplay.units.ghost;

public struct Ghost
{
    
}

public sealed class GhostSystem : IGameLogicSystem
{
    public string DebugName() => nameof(GhostSystem);

    private IFilter filter;
    private ECSWorld world;

    private GameplayFilters filters;
    private GameMap map;

    [Inject]
    private void Inject(ECSWorld world, GameplayFilters filters, GameMap map)
    {
        this.world = world;
        this.filters = filters;
        this.map = map;
        
        filter = world.Filter()
            .In<Ghost>()
            .In<EnemyUnit>()
            .Evaluate();
    }


    public void Process()
    {
        foreach (int entity in filter.Entities)
        {
            if (world.GetFrom<EnemyUnit>(entity).ReadyToTakeTurn)
            {
                MakeMove(entity);
                world.GetFrom<EnemyUnit>(entity).ReadyToTakeTurn = false;
            }
        }
    }

    private void MakeMove(int entity)
    {
        List<Vec2I> bestPath = null;
        foreach (int playerUnit in filters.PlayerUnits.Entities)
        {
            if (UTL.TryGetCellPosition(world, entity, out var pStart) &&
                UTL.TryGetCellPosition(world, playerUnit, out var pEnd))
            {
                var path = Pathfinder.FindPath(map, pStart, pEnd);
                if (bestPath == null || path.Count < bestPath.Count)
                    bestPath = path;
            }
        }

        if (bestPath == null)
        {
            LGR.PERR($"Something went wrong while defining ghost {entity} target path. Path was not found");
            return;
        }
    }
}