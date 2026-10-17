using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.gameplay.overlay;
using mix.gameplay.systems;

namespace mix.code_base.gameplay.systems;

public class AnimationCursorReactionSystem : ISystem
{
    public string DebugName() => nameof(AnimationCursorReactionSystem);

    [Inject]
    private void Inject(ECSWorld world)
    {
        this.w = world;
        world
            .Filter()
            .In<Animator>()
            .In<Hovered>()
            .In<CursorAnimationReaction>()
            .Subscribe(UpdateHover)
            .Evaluate();
    }

    private IEnumerable<int> currentHover;
    private ECSWorld w;

    private void UpdateHover(IEnumerable<int> entities)
    {
        if (currentHover != null)
        {
            foreach (int entity in currentHover)
            {
                if (w.HasIn<Animator>(entity))
                {
                    w.Pool<Animator>().Get(entity).Play(w.Pool<CursorAnimationReaction>().Get(entity).Idle);
                }
            }
        }

        currentHover = entities;
        foreach (int entity in currentHover)
        {
            w.Pool<Animator>().Get(entity).Play(w.Pool<CursorAnimationReaction>().Get(entity).Hovered);
        }
    }
}