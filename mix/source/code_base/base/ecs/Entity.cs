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
}