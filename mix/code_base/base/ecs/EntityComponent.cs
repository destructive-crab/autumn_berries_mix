namespace destruct.ecs;

public struct EntityComponent<TComponent> 
    where TComponent : struct
{
    public static readonly int TypeIndex;
    public static readonly Type Type;

    static EntityComponent()
    {
        TypeIndex = Interlocked.Increment(ref EntityComponentPools.TypesCount);
        Type = typeof(TComponent);
    }
}