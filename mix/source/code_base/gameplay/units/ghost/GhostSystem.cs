using autumn_berries_mix;
using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using mix.code_base.gameplay.systems;
using mix.gameplay.overlay.tweens;

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
            ref EnemyUnit ghostUnit = ref world.GetFrom<EnemyUnit>(entity);
            
            if (ghostUnit.ReadyToTakeTurn)
            {
                ghostUnit.ReadyToTakeTurn = false;
                MakeMove(entity);
            }

            if (ghostUnit.ShowTurnPlan)
            {
                ghostUnit.ShowTurnPlan = false;
            }
        }
    }

    private async Task MakeMove(int entity)
    {
        List<Vec2I> bestPath = null;
        
        if (!UTL.TryGetCellPosition(world, entity, out var pStart))
        {
            LGR.PERR($"Failed getting cell position from ghost with id {entity}");
            return;
        }

        foreach (int playerUnit in filters.PlayerUnits.Entities)
        {
            LGR.PM($"TRYING PLAYER UNIT {playerUnit} FOR GHOST");
            if (UTL.TryGetCellPosition(world, playerUnit, out var pEnd))
            {
                List<Vec2I> path = Pathfinder.FindPath(map, pStart, pEnd);
                if (bestPath == null || path.Count < bestPath.Count)
                    bestPath = path;
            }
            else
            {
                LGR.PW($"Player Unit {playerUnit} has no cell position");
            }
        }

        if (bestPath == null)
        {
            LGR.PERR($"Something went wrong while defining ghost {entity} target path. Path was not found");
            return;
        }

        Vec2I moveDir = bestPath[0] - pStart;
        Vec2I currentCell = pStart;
        
        for (int i = 0; i < map.CellsCount; i++)
        {
            Vec2I nextCell = currentCell + moveDir;

            if (map.CellHasEntityWith<BlockCell>(nextCell))
                if (!map.CellHasEntityWith<Health>(nextCell)) break;
            
            foreach (var entityToHit in map.GetEntitiesWith<Health>(nextCell))
            {
                world.GetFrom<Health>(entityToHit).Current -= 1;
            }

            bool completed = false;
            world.Pool<PositionTween>().New(entity, (ref PositionTween t) =>
            {
                t.Destination = nextCell;
                t.Speed = 2;
    
                t.Completed += () =>
                {
                    completed = true;
                }; 
            });

            while (!completed)
            {
                await Task.Yield();
            }

            currentCell = nextCell;
        }
        
        map.MoveTile(entity, currentCell);
        world.GetFrom<EnemyUnit>(entity).ReadyToTakeTurn = false;
    }
}