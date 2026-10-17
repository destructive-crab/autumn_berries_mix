namespace destruct.ecs;

public sealed class ECSWorld
{
    private readonly Dictionary<int, IEntityComponentPool> pools = new();
    private EntityData[] entities;
    
    private int lastEntityId = 0;
    
    private readonly Queue<int> freeEntities = new();

    private readonly List<Filter> filters = new();
    private bool filtersEvaluationBlocked = false;

    public ECSWorld()
    {
        entities = new EntityData[1000];
        
        for (var i = 0; i < entities.Length; i++)
        {
            entities[i] = new();
        }
    }

    public int EntitiesCount()
    {
        return lastEntityId - freeEntities.Count + 1;
    }

    public Entity New()
    {
        int index = -1;
        if (freeEntities.TryDequeue(out var freeEntityId))
        {
            index = freeEntityId;
        }
        else
        {
            index = lastEntityId++;
        
            if (index >= entities.Length)
                Array.Resize(ref entities, entities.Length * 2);
        }
        
        Entity entity = new(index, this);
        entities[index].World = this;
        
        return entity;
    }

    public void Delete(int entityId)
    {
        if (entityId < 0 || entityId > lastEntityId)
            throw new Exception($"Invalid entity id: {entityId}");
        if (entities[entityId].World == null)
            throw new Exception("Entity not created or was destroyed");

        for (var i = 0; i < entities[entityId].Components.Count; i++)
        {
            var component = entities[entityId].Components[i];
            pools[component].Remove(entityId);
            i--;
        }

        entities[entityId].World = null;
    }

    public IEntityComponentPool Pool(int index)
    {
        return pools[index];
    }

    public ref TComponent GetFrom<TComponent>(int entityId) 
        where TComponent : struct 
        => ref Pool<TComponent>().Get(entityId);

    public bool HasIn<TComponent>(int entityId)
        where TComponent : struct
    {
        return Pool<TComponent>().Has(entityId);
    }

    public void RemoveFrom<TComponent>(int entity)
        where TComponent : struct
    {
        Pool<TComponent>().Remove(entity);
    }
    
    public EntityComponentPool<TComponent> Pool<TComponent>()
        where TComponent : struct
    {
        int typeIndex = EntityComponent<TComponent>.TypeIndex;
        
        if (!pools.ContainsKey(typeIndex))
        {
            EntityComponentPool<TComponent> pool = new(entities.Length / 2, entities.Length / 2, this);
            pools.Add(typeIndex, pool);
        }
        
        return pools[typeIndex] as EntityComponentPool<TComponent>;
    }
    

    public IFilter Filter()
    {
        var filter = new Filter(this);
        filters.Add(filter);
        return filter;
    }

    public bool EntityHas<TComponent>(int entityId)
        where TComponent : struct
    {
         int typeIndex = EntityComponent<TComponent>.TypeIndex;
         return GetData(entityId).Components.Contains(typeIndex);
    }

    public bool EntityHas<TComponent>(in Entity entity)
        where TComponent : struct
    {
        return EntityHas<TComponent>(entity.Id);
    }

    public ref EntityData GetData(int entityId)
    {
        return ref entities[entityId];
    }

    public ref EntityData GetData(in Entity entity)
    {
        return ref entities[entity.Id];
    }

    public void AddComponentFor(int entityId, int componentIndex)
    {
        GetData(entityId).Components.Add(componentIndex);
        EvaluateFiltersWith(componentIndex);
    }

    public void RemoveComponentFor(int entityId, int componentIndex)
    {
        GetData(entityId).Components.Remove(componentIndex);
        EvaluateFiltersWith(componentIndex);
    }

    public void BlockFiltersEvaluation()
    {
        filtersEvaluationBlocked = true;
    }

    public void UnblockFiltersEvaluationAndEvaluate()
    {
        filtersEvaluationBlocked = false;
        EvaluateAllFilters();
    }

    public void EvaluateAllFilters()
    {
        foreach (Filter filter in filters)
        {
            EvaluateFilter(filter);
        }
    }

    private void EvaluateFiltersWith(int componentIndex)
    {
        foreach (Filter filter in filters)
            if (filter.IncludedComponents.Contains(componentIndex) || filter.ExcludedComponents.Contains(componentIndex))
                filter.Evaluate();
    }

    public void EvaluateFilter(Filter filter)
    {
        if (filtersEvaluationBlocked) return;

        var changed = true;
        filter.TrackIsChanged();
        
        for (var i = 0; i <= lastEntityId; i++)
        {
            if (entities[i].World != null)
            {
                changed = filter.AddEntityAndTrackChanges(i, entities[i]);
            }
        }

        filter.FinishTracking();
        
        if(changed)
        {
            filter.InformSubscribers();
        }
    }
}