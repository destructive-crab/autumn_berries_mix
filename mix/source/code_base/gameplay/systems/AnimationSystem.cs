using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using Raylib_cs;
using Sprite = mix.code_base.Sprite;

namespace mix.gameplay.systems;

public struct AnimationClipFrame
{
    public CachedTexture Texture;

    public int FrameDurationMS;

    public Rectangle Rect;

    public AnimationClipFrame() {}
    
    public AnimationClipFrame(CachedTexture texture, int frameDurationMs, Rectangle rect)
    {
        Texture = texture;
        FrameDurationMS = frameDurationMs;
        Rect = rect;
    }

    public AnimationClipFrame(CachedTexture texture, int x, int y, int w, int h)
    {
        FrameDurationMS = 200;
        Texture = texture;
        Rect = new(x, y, w, h);
    }
}

public struct AnimationClip
{
    public string Name;
    public AnimationClipFrame[] Frames;
    public bool Cycle;
}

public struct Animator
{
    public AnimationClip Current;

    public int ClipFrame;
    public int MSLeftToNextFrame;
}

public sealed class AnimationSystem : ISceneSystem, ISceneSystemTick
{
    public string DebugName() => nameof(AnimationSystem);

    private ECSWorld world;
    private IEnumerable<int> animatedSprites;
    private EntityComponentPool<Animator> animatorsPool;

    [Inject] 
    private void Inject(ECSWorld world)
    {
        this.world = world;
        animatorsPool = world.Pool<Animator>();
        
        world.Filter()
            .In<Animator>()
            .In<Sprite>()
            .Subscribe(FilterEvaluated)
            .Evaluate();
    }

    private void FilterEvaluated(IEnumerable<int> entities)
    {
        animatedSprites = entities;
    }

    public void Tick()
    {
        int deltaTimeMs = (int)(Game.DeltaTime * 1000);
        
        foreach (int animatedSprite in animatedSprites)
        {
            ref Animator animator = ref animatorsPool.Get(animatedSprite);
            animator.MSLeftToNextFrame -= deltaTimeMs;
            if (animator.MSLeftToNextFrame <= 0)
            {
                if (animator.ClipFrame < animator.Current.Frames.Length - 1)
                {
                    ref Sprite sprite = ref world.Pool<Sprite>().Get(animatedSprite);
                    animator.ClipFrame++;
                    animator.MSLeftToNextFrame = animator.Current.Frames[0].FrameDurationMS;
                    sprite.Texture = animator.Current.Frames[animator.ClipFrame].Texture;
                    sprite.Rect = animator.Current.Frames[animator.ClipFrame].Rect;
                }
                else if (animator.Current.Cycle)
                {
                    animator.ClipFrame = -1;
                }
            }
        }
    }
}