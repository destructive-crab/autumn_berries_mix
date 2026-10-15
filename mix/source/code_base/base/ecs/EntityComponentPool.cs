using destruct.Debug;

namespace destruct.ecs;

public sealed class EntityComponentPool<TComponent> : IEntityComponentPool
    where TComponent : struct
{
    public Type Type { get; }
    
    private TComponent[] components;
    private int[] entityToComponent;
    private readonly Queue<int> freeComponents = new();

    private int entitiesCapacity;
    private int poolCapacity;

    private int componentsCount = 0;

    public event Action<int> OnChanged;
    
    public event Action<EntityComponentPool<TComponent>, int> OnNew;
    public event Action<EntityComponentPool<TComponent>, int> OnRemoved;
    
    private readonly ECSWorld world;

    public EntityComponentPool(int poolCapacity, int entitiesCapacity, ECSWorld world)
    {
        Type = typeof(TComponent);
        
        this.entitiesCapacity = entitiesCapacity;
        this.world = world;
        this.poolCapacity = poolCapacity;

        components = new TComponent[poolCapacity];
        entityToComponent = new int[entitiesCapacity];
    }

    private int GetNewComponentIndexFor(int entityId)
    {
        if (entityToComponent.Length <= entityId)
        {
            entitiesCapacity *= 2;
            Array.Resize(ref entityToComponent, entitiesCapacity);
        }
        
        if (entityToComponent[entityId] != 0)
            throw new Exception($"Component already added to {entityId}");
        
        int index = -1;
        if (freeComponents.Count > 0)
        {
            index = freeComponents.Dequeue();
        }
        else
        {
            componentsCount++;
            index = componentsCount;

            if (index >= components.Length)
            {
                poolCapacity *= 2;
                Array.Resize(ref components, poolCapacity);
                LGR.PM($"Pool capacity increased {poolCapacity} {components.Length} for index {index}");
            }
        }

        return index;
    }

    public void Changed(int entityId)
    {
        OnChanged?.Invoke(entityId);
    }
    
    public int New(int entityId)
    {
        int index = GetNewComponentIndexFor(entityId);
        
        entityToComponent[entityId] = index;
        
        OnNew?.Invoke(this, entityId);

        world.AddComponentFor(entityId, EntityComponent<TComponent>.TypeIndex);
        
        return index;
    }

    public bool Has(int entityId)
    {
        return entityToComponent.Length > entityId && entityToComponent[entityId] != 0;
    }
    
    public void Remove(int entityId)
    {
        if (entityToComponent[entityId] == -1)
        {
            throw new Exception($"No component attached to {entityId}. Can not remove it");
        }

        components[entityToComponent[entityId]] = default;
        freeComponents.Enqueue(entityToComponent[entityId]);
        entityToComponent[entityId] = 0;
        
        world.RemoveComponentFor(entityId, EntityComponent<TComponent>.TypeIndex);
        OnRemoved?.Invoke(this, entityId);
    }

    public ref TComponent New(int entityId, ComponentConfiguration<TComponent> configuration)
    {
        int index = GetNewComponentIndexFor(entityId);
        
        entityToComponent[entityId] = index;
        configuration?.Invoke(ref components[index]);
        
        OnNew?.Invoke(this, entityId);

        world.AddComponentFor(entityId, EntityComponent<TComponent>.TypeIndex);
        
        return ref components[index];
    }
    
    public ref TComponent New(Entity entity, ComponentConfiguration<TComponent> configuration = null)
        => ref New(entity.Id, configuration);

    public object GetItem(int entityId)
    {
        return components[entityToComponent[entityId]];
    }

    public ref TComponent Get(int entityId)
    {
        if (entityToComponent[entityId] == 0)
            throw new Exception($"No component attached to {entityId}");

        return ref components[entityToComponent[entityId]];
    }
}