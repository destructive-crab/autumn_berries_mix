using destruct;
using destruct.DI;
using destruct.ecs;

namespace mix.code_base;

public sealed class GameMap 
{
    private readonly Dictionary<Vec2I, List<int>> map = new();
    
    [Inject] private ECSWorld world;
    
    public void UpdateLayout(IEnumerable<int> entities)
    {
        map.Clear();
        foreach (int entity in entities)
        {
            Position posComponent = world.Pool<Position>().Get(entity);
            
            Vec2I cellPos = new Vec2I((int)Math.Round(posComponent.X), (int)Math.Round(posComponent.Y));
            
            map.TryAdd(cellPos, new());
            map[cellPos].Add(entity);
        }
    }
    
    public bool Contains(Vec2I pointedCell)
    {
        return map.ContainsKey(pointedCell);
    }

    public bool CellHasEntityWith<TComponent>(Vec2I position) where TComponent : struct
    {
        if (!map.ContainsKey(position))
            throw new Exception("No cell with position {position] found in map");

        EntityComponentPool<TComponent> pool = world.Pool<TComponent>();
        
        foreach (int entityId in map[position])
            if (pool.Has(entityId)) return true;

        return false;
    }

    public IEnumerable<int> GetEntitiesAt(Vec2I position)
    {
        if (!map.ContainsKey(position))
            throw new Exception("No cell with position {position] found in map");

        return map[position];
    }

    public Memory<Vec2I> GetExistingCellsAround(Vec2I position)
    {
        Vec2I[] result = new Vec2I[8];
        int resLength = 0;
        
        Vec2I[] offsets = UTL.GetAllOffsets();
        for (int i = 0; i < offsets.Length; i++)
        {
            if (Contains(position + offsets[i]))
            {
                result[resLength] = position + offsets[i];
            }
        }

        return new Memory<Vec2I>(result, 0, resLength);
    }
    
    public IEnumerable<Vec2I> GetExistingCellsAroundPositionThatHaveEntityWith<TComponent>(Vec2I position)
        where TComponent : struct
    {
        List<Vec2I> result = new(8);
        Vec2I[] offsets = UTL.GetAllOffsets();
        
        for (int i = 0; i < offsets.Length; i++)
        {
            var cellPosition = position + offsets[i];
            if (Contains(cellPosition) && CellHasEntityWith<TComponent>(position))
            {
                result.Add(cellPosition);
            }
        }

        return result;
    }

    public IEnumerable<Vec2I> GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<TComponent>(int x, int y)
        where TComponent : struct
        => GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<TComponent>(x, y);
    
    public IEnumerable<Vec2I> GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<TComponent>(Vec2I position)
        where TComponent : struct
    {
        List<Vec2I> result = new(8);
        
        Vec2I[] offsets = UTL.GetAllOffsets();
        for (int i = 0; i < offsets.Length; i++)
        {
            var cellPosition = position + offsets[i];
            if (Contains(cellPosition) && !CellHasEntityWith<TComponent>(position))
            {
                result.Add(cellPosition);
            }
        }

        return result;
    }
}