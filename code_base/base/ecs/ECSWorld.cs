using System;
using System.Collections.Generic;

namespace destruct.ecs;

public sealed class ECSWorld
{
    private readonly Dictionary<MapFilter, Action<int[]>> filters = new();

    private readonly Dictionary<int, IEntityComponentPool> pools = new();
    private EntityData[] entities;
    private bool[] dirtyEntites;
    
    private int lastEntityId = 0;
    
    private readonly Queue<int> freeEntities = new();

    public ECSWorld()
    {
        entities = new EntityData[1000];
        
        for (var i = 0; i < entities.Length; i++)
        {
            entities[i] = new();
        }
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
        
        UpdateFilters();
        
        return entity;
    }

    public void Delete(int entityId)
    {
        if (entityId < 0 || entityId > lastEntityId)
            throw new Exception($"Invalid entity id: {entityId}");
        if (entities[entityId].World == null)
            throw new Exception("Entity not created or was destroyed");
        
        foreach (int component in entities[entityId].Components)
        {
            pools[component].Remove(entityId);
        }

        entities[entityId].World = null;
    }

    public EntityComponentPool<TComponent> Pool<TComponent>()
        where TComponent : struct
    {
        int typeIndex = TileComponent<TComponent>.TypeIndex;
        
        if (!pools.ContainsKey(typeIndex))
        {
            EntityComponentPool<TComponent> pool = new(entities.Length / 2, entities.Length / 2, this);
            pool.OnChanged += UpdateFilters;
            pools.Add(typeIndex, pool);
        }
        
        return pools[typeIndex] as EntityComponentPool<TComponent>;
    }

    public bool EntityHas<TComponent>(int entityId)
        where TComponent : struct
    {
         int typeIndex = TileComponent<TComponent>.TypeIndex;
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

    public void RegFilter(MapFilter filter, Action<int[]> callback)
    {
        filters.Add(filter, callback);
        UpdateFilter(filter, callback);
    }

    private void UpdateFilters()
    {
        foreach (var pair in filters)
        {
            UpdateFilter(pair.Key, pair.Value);
        }
    }

    private void UpdateFilter(MapFilter filter, Action<int[]> callback)
    {
        List<int> result = new();
        for (var i = 0; i < entities.Length; i++)
        {
            if (filter.IsCompatible(this, i))
            {
                result.Add(i);
            }
        }

        callback?.Invoke(result.ToArray());
    }

    public void AddComponentFor(int entityId, int componentIndex)
    {
        GetData(entityId).Components.Add(componentIndex);
        UpdateFilters();
    }

    public void RemoveComponentFor(int entityId, int componentIndex)
    {
        GetData(entityId).Components.Remove(componentIndex);
        UpdateFilters();
    }
}