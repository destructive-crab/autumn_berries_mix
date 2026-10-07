using destruct.Debug;

namespace destruct.ecs;

public sealed class Filter : IFilter
{
    public bool ReactOnComponentDataChanges;

    public string DebugName { get; private set; }
    
    public IEnumerable<int> Entities => entities;
    private List<int> entities = new();

    public IEnumerable<int> IncludedComponents => includedComponents;
    private readonly List<int> includedComponents = new();
    
    public IEnumerable<int> ExcludedComponents => excludedComponents;
    private readonly List<int> excludedComponents = new();

    private readonly ECSWorld world;
    
    private readonly List<Action<IEnumerable<int>>> subscribers = new();
    private readonly Dictionary<int, List<Action<IEnumerable<int>>>> dataChangesSubscribers = new();

    public Filter(ECSWorld world)
    {
        this.world = world;
    }

    public IFilter In<TComponent>()
        where TComponent : struct
    {
        includedComponents.Add(EntityComponent<TComponent>.TypeIndex);
        DebugName += $"In[{typeof(TComponent).Name}] ";
        return this;
    }

    public IFilter Ex<TComponent>()
        where TComponent : struct
    {
        excludedComponents.Add(EntityComponent<TComponent>.TypeIndex);
        DebugName += $"Ex[{typeof(TComponent).Name}] ";
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

    public bool AddEntityAndTrackChanges(int id, EntityData entity)
    {
        if (!trackNewEvaluationChanges)
        {
            throw new Exception("You need to begin evaluation changes tracking before processing entities");
        }

        if (!IsCompatible(entity)) return changed;
        
        newEvaluation.Add(id);
        changed = entities.Count != newEvaluation.Count;
        
        if (!changed)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                //because entities are stored in array using indexes and they are evaluated using for loop on entities array 
                //then we can be sure that in filter's list they will be always in same order
                changed = entities[i] != newEvaluation[i];
                if (changed) break;
            }
        }

        return changed;
    }
    
    public bool AddEntityIfCompatible(int id, EntityData entity)
    {
        if (IsCompatible(entity))
        {
            entities.Add(id);
        }

        return changed;
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
    
    public void InformSubscribersAboutChanges<TComponent>(int entityId) 
        where TComponent : struct
    {
        int[] entities = [entityId];
        foreach (var subscriber in dataChangesSubscribers[EntityComponent<TComponent>.TypeIndex])
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

    public IFilter SubscribeOnDataChanges<TComponent>(Action<IEnumerable<int>> callback) 
        where TComponent : struct
    {
        //todo unsubscribe
        world.Pool<TComponent>().OnChanged += InformSubscribersAboutChanges<TComponent>;
        
        int index = EntityComponent<TComponent>.TypeIndex;
        dataChangesSubscribers.TryAdd(index, new());
        dataChangesSubscribers[index].Add(callback);
        
        return this;
    }

    public override bool Equals(object obj)
    {
        return obj is Filter mapFilter &&
               mapFilter.IncludedComponents == IncludedComponents &&
               mapFilter.ExcludedComponents == ExcludedComponents;
    }

    private bool trackNewEvaluationChanges;
    private bool changed;
    private List<int> newEvaluation;
    public void TrackIsChanged()
    {
        trackNewEvaluationChanges = true;
        changed = false;
        newEvaluation = new();
        newEvaluation.Clear();
    }

    public void FinishTracking()
    {
        trackNewEvaluationChanges = false;
        entities = newEvaluation;
    }
}