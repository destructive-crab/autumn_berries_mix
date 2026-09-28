namespace destruct.ecs;

public sealed class Filter : IFilter
{
    public IEnumerable<int> Entities => entities;
    private readonly List<int> entities = new();

    public IEnumerable<int> IncludedComponents => includedComponents;
    private readonly List<int> includedComponents = new();
    
    public IEnumerable<int> ExcludedComponents => excludedComponents;
    private readonly List<int> excludedComponents = new();

    private readonly ECSWorld world;
    private readonly List<Action<IEnumerable<int>>> subscribers = new();

    public Filter(ECSWorld world)
    {
        this.world = world;
    }

    public IFilter In<TComponent>()
        where TComponent : struct
    {
        includedComponents.Add(EntityComponent<TComponent>.TypeIndex);
        return this;
    }

    public IFilter Ex<TComponent>()
        where TComponent : struct
    {
        excludedComponents.Add(EntityComponent<TComponent>.TypeIndex);
        return this;
    }
    
    public bool IsCompatible(EntityData data)
    {
        foreach (int component in includedComponents)
        {
            if (!data.Components.Contains(component)) 
                return false;
        }
        
        foreach (int component in excludedComponents)
        {
            if (data.Components.Contains(component)) 
                return false;
        }
        
        return true;
    }

    public void AddEntityIfCompatible(int id, EntityData entity)
    {
        if (IsCompatible(entity))
        {
            entities.Add(id);
        }
    }

    public void Clear()
    {
        entities.Clear();
    }

    public void InformSubscribers()
    {
        foreach (var subscriber in subscribers)
        {
            subscriber.Invoke(entities);
        }
    }
    
    public IFilter Evaluate()
    {
        world.EvaluateFilter(this);
        return this;
    }

    public IFilter Subscribe(Action<IEnumerable<int>> callback)
    {
        subscribers.Add(callback);
        return this;
    }

    public override bool Equals(object obj)
    {
        return obj is Filter mapFilter &&
               mapFilter.IncludedComponents == IncludedComponents &&
               mapFilter.ExcludedComponents == ExcludedComponents;
    }
}