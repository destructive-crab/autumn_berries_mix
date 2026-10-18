using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using mix.code_base;

namespace mix.gameplay.overlay.units.abilities;

public sealed class AreaTracker
{
    private readonly Dictionary<int, HashSet<Vec2I>> areas = new();
    private readonly Dictionary<int, HashSet<int>> dependencies = new();
    
    [Inject] private GameMap map;
    [Inject] private ECSWorld world;

    public AreaTracker() {}
    public AreaTracker(GameMap map, ECSWorld world)
    {
        this.map = map;
        this.world = world;
    }

    public IEnumerable<Vec2I> TrackWith<TComponent>(int id, Vec2I origin, Predicate<Vec2I> valid, int radius = 1)
        where TComponent : struct
    {
        HashSet<Vec2I> hashSet = new();
        areas.Add(id, hashSet);
        
        foreach (var c in map.GetExistingCellsAroundPositionThatHaveEntityWith<TComponent>(origin, radius))
            if (valid.Invoke(c)) hashSet.Add(c);
        
        return hashSet;
    }
    
    public IEnumerable<Vec2I> TrackWithout<TComponent>(int id, Vec2I origin, Predicate<Vec2I> valid, int radius = 1)
        where TComponent : struct
    {
        HashSet<Vec2I> hashSet = new();
        areas.Add(id, hashSet);
        
        foreach (var c in map.GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<TComponent>(origin, radius))
            if (valid.Invoke(c)) hashSet.Add(c);

        return hashSet;
    }

    public void RegisterDependenceThatWillBeDeletedWhenTrackStoppedFor(int id, int dependence)
    {
        dependencies.TryAdd(id, new());
        dependencies[id].Add(dependence);
    }
    
    public void RegisterDependenciesThatWillBeDeletedWhenTrackStoppedFor(int id, IEnumerable<int> dependencies)
    {
        this.dependencies.TryAdd(id, new());
        foreach (int d in dependencies)
        {
            this.dependencies[id].Add(d);
        }
    }

    public void StopTrack(int id)
    {
        if (!areas.ContainsKey(id))
        {
            LGR.PERR($"Trying to stop tracking area with id {id}. But there are no such id");
            return;
        }
        
        areas.Remove(id);
        
        if (!dependencies.ContainsKey(id)) return;
            
        foreach (int dependence in dependencies[id])
        {
            world.Delete(dependence);
        }

        dependencies.Remove(id);
    }

    public void Foreach(Func<int, IEnumerable<Vec2I>, bool> callback)
    {
        foreach (var pair in areas)
            callback.Invoke(pair.Key, pair.Value);
    }
    
    public void Foreach(Func<int, Vec2I, bool> callback)
    {
        foreach (var pair in areas)
        foreach (Vec2I cellPosition in pair.Value)
            callback.Invoke(pair.Key, cellPosition);
    }

    public bool TryGetAreaIdIfCellInIt(Vec2I position, out int id)
    {
        id = -1;
        foreach (var pair in areas)
        {
            foreach (Vec2I cellPosition in pair.Value)
            {
                if (cellPosition == position)
                {
                    id = pair.Key;
                    return true;
                }
            }
        }

        return false;
    }
}