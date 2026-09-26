namespace destruct.ecs;

public interface IEntityComponentPool
{
    Type Type { get; }
    
    object GetItem(int index);
    int New(int entityId);
    void Remove(int entityId);
}