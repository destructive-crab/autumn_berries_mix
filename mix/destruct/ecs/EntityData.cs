namespace destruct.ecs;

public struct EntityData
{
    public ECSWorld World;
    public readonly List<int> Components = new();

    public EntityData()
    {
        World = null;
        Components = new();
    }
}