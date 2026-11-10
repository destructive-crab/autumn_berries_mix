using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using destruct;

namespace mix.gameplay.map;

public sealed class GameMapSystem : ISystem
{
    public string DebugName() => nameof(GameMap);
    
    [Inject]
    private void Construct(ECSWorld world, GameMap map)
    {
        world.Filter()
            .In<Tile>()
            .In<Position>()
            .Subscribe(map.UpdateLayout)
            .Evaluate();
    }
}