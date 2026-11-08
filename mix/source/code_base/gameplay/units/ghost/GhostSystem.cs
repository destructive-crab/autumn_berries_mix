using autumn_berries_mix;
using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.gameplay.systems;
using destruct.tweens;

namespace destruct.gameplay.units.ghost;

public struct Ghost
{
    public int Damage;
}

public sealed class GhostSystem : IGameLogicSystem
{
    public string DebugName() => nameof(GhostSystem);

    private IFilter filter;
    private ECSWorld world;

    private GameplayFilters filters;
    private GameMap map;

    private const int GhostMoveRecursionLimit = 3;

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
        await MakeMove(entity, pStart, moveDir);
    }

    private async Task MakeMove(int entity, Vec2I start, Vec2I moveDir, int howManyTimesAlreadyWasRepeated = 0)
    {
        try
        {

        if (howManyTimesAlreadyWasRepeated == 0) map.QuitMap(entity);
        
        world.GetOrAdd(entity, (ref RotationTween t) => 
        {
            t.Speed = 80;
            int sign = Math.Sign(moveDir.X);
            if (sign == 0) sign = 1;
            t.DestinationValue =  sign * 10;
        });
            
        Vec2I currentCell = start;

        world.GetFrom<Sprite>(entity).MirroredHorizontal = moveDir.X < 0;
        
        for (int i = 0; i < map.CellsCount; i++)
        {
            Vec2I nextCell = currentCell + moveDir;

            if (howManyTimesAlreadyWasRepeated > GhostMoveRecursionLimit)
            {
                if (map.IsCellWalkable(nextCell)) await PerformMoveTween(nextCell);

                break;
            }
            
            bool hasEntityToHit = map.Contains(nextCell) && map.CellHasEntityWith<Health>(nextCell);
            
            if (!map.IsCellWalkable(nextCell) && !hasEntityToHit) break;
            
            currentCell = await PerformMoveTween(nextCell);
            
            if (hasEntityToHit)
            {
                foreach (var entityToHit in map.GetEntitiesWith<Health>(nextCell))
                {
                    HealthUTL.Hit(world, entityToHit, world.GetFrom<Ghost>(entity).Damage);
                }

                Vec2I nextNextCell = currentCell + moveDir;
                if (!map.Contains(nextNextCell) ||
                   (!map.IsCellWalkable(nextNextCell) && 
                    !map.CellHasEntityWith<Health>(nextNextCell)))
                {
                    await MakeMove(entity, currentCell, -moveDir, howManyTimesAlreadyWasRepeated+1);
                    return;
                }
            }
        }

        if (world.HasIn<RotationTween>(entity))
        {
            world.GetFrom<RotationTween>(entity).DestinationValue = 0;
        }
        else
        {
            world.Pool<RotationTween>().New(entity, (ref RotationTween t) =>
            {
                t.Speed = 80;
                t.DestinationValue = 0;
            });
        }
        map.AddTileToCell(entity, currentCell);
        world.GetFrom<EnemyUnit>(entity).ReadyToTakeTurn = false;
            
        async Task<Vec2I> PerformMoveTween(Vec2I nextCell)
        {
            await Tweens.MoveTween(world, entity, nextCell, 3);

            currentCell = nextCell;
            return currentCell;
        }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }
}