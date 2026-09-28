namespace destruct.ecs;

public interface IFilter
{
    IEnumerable<int> Entities { get; }
    
    IEnumerable<int> IncludedComponents { get; }
    IEnumerable<int> ExcludedComponents { get; }

    IFilter In<TComponent>()
        where TComponent : struct;

    IFilter Ex<TComponent>()
        where TComponent : struct;

    IFilter Evaluate();

    IFilter Subscribe(Action<IEnumerable<int>> callback);
}