using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;

namespace mix.gameplay.overlay.models;

public sealed class GameMapSystem : ISceneSystem
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