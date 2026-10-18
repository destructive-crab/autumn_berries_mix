using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;

namespace mix.code_base.gameplay.systems;

public class SelectionSoundSystem : ISystem
{
    public string DebugName() => nameof(SelectionSoundSystem);

    [Inject]
    private void Inject(ECSWorld world)
    {
        world.Pool<Selected>().OnNew += OnSelected;
    }

    private void OnSelected(EntityComponentPool<Selected> pool, int entityId)
    {
        Game.AudioService.PlaySound("assets/audio/click.ogg", 0.4f, Random.Shared.Next(90, 101)/100f);
    }
}