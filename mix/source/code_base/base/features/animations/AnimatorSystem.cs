using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;

namespace mix.gameplay.systems;

public sealed class AnimatorSystem : IPreRendererSystem
{
    public string DebugName() => nameof(AnimatorSystem);
    
    private IFilter filter;
    private ECSWorld world;

    [Inject]
    private void Inject(ECSWorld world)
    {
        this.world = world;
        filter = world.Filter()
            .In<Animator>()
            .In<SpriteAnimation>();
    } 
    
    public void PreRenderer()
    {
        foreach (int entity in filter.Entities)
        {
            ref Animator animator = ref world.GetFrom<Animator>(entity);
            foreach (AnimatorState state in animator.States)
            {
                if (state.Validate(ref animator))
                {
                    if (animator.CurrentState != state.Id)
                    {
                        world.GetFrom<SpriteAnimation>(entity)
                             .SetClip(animator.Container.Get(state.ClipName));
                        
                        animator.CurrentState = state.Id;
    
                        foreach (string trigger in state.Triggers)
                            animator.Triggers.Remove(trigger);
                    }

                    break;
                }
            }
        }
    }
}