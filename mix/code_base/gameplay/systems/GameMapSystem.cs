using destruct.DI;
using destruct.ecs;
using destruct.scenes;

namespace mix.code_base;

public sealed class GameMapSystem : ISceneSystem
{
    public string DebugName() => nameof(GameMapSystem);

    private ECSWorld world;

    private readonly Dictionary<Vec2I, List<int>> map = new();

    [Inject]
    private void Construct(ECSWorld world)
    {
        this.world = world;
        world.Filter()
            .In<Tile>()
            .In<Position>()
            .Subscribe(UpdateLayout)
            .Evaluate();
    }
    
    private void UpdateLayout(IEnumerable<int> entities)
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
}