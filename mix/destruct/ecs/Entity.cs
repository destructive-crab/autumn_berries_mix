namespace destruct.ecs;

public struct Entity
{
    public readonly int Id;
    public readonly ECSWorld Owner;

    public Entity(int id, ECSWorld owner)
    {
        Id = id;
        Owner = owner;
    }

    public Entity With<TComponent>() 
        where TComponent : struct
    {
        Owner.Pool<TComponent>().New(Id);
        return this;
    }
    
    public Entity With<TComponent>(ComponentConfiguration<TComponent> configuration) 
        where TComponent : struct
    {
        Owner.Pool<TComponent>().New(Id, configuration);
        return this;
    }
    
    public Entity With<TComponent>(TComponent component) 
        where TComponent : struct
    {
        Owner.Pool<TComponent>().New(Id, (ref TComponent c) => c = component);
        return this;
    }

    public override string ToString()
    {
        return $"Entity {Id}";
    }
}