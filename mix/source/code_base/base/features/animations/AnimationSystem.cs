using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using Sprite = mix.code_base.Sprite;

namespace mix.gameplay.systems;

public sealed class AnimationSystem : ISystem, IWorldRendererSystem
{
    public string DebugName() => nameof(AnimationSystem);

    private ECSWorld world;
    private EntityComponentPool<SpriteAnimation> animatorsPool;
    
    private IEnumerable<int> animatedSprites = [];

    [Inject] 
    private void Inject(ECSWorld world)
    {
        this.world = world;
        animatorsPool = world.Pool<SpriteAnimation>();
        
        world.Filter()
            .In<SpriteAnimation>()
            .In<Sprite>()
            .Subscribe(FilterEvaluated)
            .Evaluate();
    }

    private void FilterEvaluated(IEnumerable<int> entities)
    {
        animatedSprites = entities;
    }

    public void RenderWorld()
    {
        float deltaTimeMs = Game.DeltaTime * 1000;
        
        foreach (int animatedSprite in animatedSprites)
        {
            ref SpriteAnimation spriteAnimation = ref animatorsPool.Get(animatedSprite);
            
            if (spriteAnimation.Paused) continue;
            if (spriteAnimation.IsEmpty()) continue;
            
            spriteAnimation.TimeLeftToNextFrame -= deltaTimeMs;
            if (spriteAnimation.TimeLeftToNextFrame <= 0)
            {
                if (spriteAnimation.ClipFrame < spriteAnimation.Clip.Frames.Length - 1)
                {
                    ref Sprite sprite = ref world.Pool<Sprite>().Get(animatedSprite);
                    spriteAnimation.ClipFrame++;
                    spriteAnimation.TimeLeftToNextFrame = spriteAnimation.Clip.Frames[0].FrameDurationMS;
                    sprite.Fragment = spriteAnimation.Clip.Frames[spriteAnimation.ClipFrame].Fragment;
                }
                else if (spriteAnimation.Clip.Cycle)
                {
                    spriteAnimation.ClipFrame = -1;
                }
            }
        }
    }
}