using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using Raylib_cs;
using Sprite = mix.code_base.Sprite;

namespace mix.gameplay.systems;

public static class AnimatorExtensions
{
    public static void InitializeFromAse(ref this Animator animator, string pathToAseFile)
    {
        animator.AnimationsContainer = Assets.LoadAnimationsContainer(pathToAseFile);
    }
    
    public static bool HasCurrentAnimation(this in Animator animator)
    {
        return animator.IsInitialized() && !string.IsNullOrEmpty(animator.Current);
    }

    public static bool IsInitialized(this in Animator animator)
    {
        return animator.AnimationsContainer != null;
    }

    public static AnimationClip Current(this in Animator animator)
    {
        if (!animator.IsInitialized())
        {
            LGR.PW("Trying to play animation on uninitialized animator");
        }

        if (!animator.HasCurrentAnimation())
        {
            LGR.PW("Trying to get current animation on animator that has no current animation");
        }

        return animator.AnimationsContainer.Get(animator.Current);
    }
    
    public static void Play(ref this Animator animator, string name)
    {
        if (!animator.IsInitialized())
        {
            LGR.PW("Trying to play animation on uninitialized animator");
            return;
        }
        
        animator.Current = name;
        animator.ClipFrame = 0;
        animator.MSLeftToNextFrame = animator.Current().Frames[0].FrameDurationMS;
    }

    public static void Pause(ref this Animator animator)
    {
        animator.Paused = true;
    }

    public static void Unpause(ref this Animator animator)
    {
        animator.Paused = false;
    }
}

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
    public AnimationsContainer AnimationsContainer;
    
    public string Current;

    public bool Paused;
    
    public int ClipFrame;
    public int MSLeftToNextFrame;
}

public sealed class AnimationsContainer
{
    private readonly Dictionary<string, AnimationClip> clips;

    public AnimationsContainer(AnimationClip[] clips)
    {
        this.clips = new Dictionary<string, AnimationClip>();
        foreach (AnimationClip clip in clips)
        {
            if (clip.Frames.Length == 0)
            {
                LGR.PW($"Clip({clip.Name}) with zero frames found. Clip will be not included in container");
                continue;
            }
            
            this.clips.Add(clip.Name, clip);
        }
    }

    public bool Contains(string clipName) => clips.ContainsKey(clipName);
    
    public AnimationClip Get(string clipName)
    {
        if (!clips.ContainsKey(clipName))
        {
            LGR.PW($"No clip with name {clipName} found in container");
        }

        return clips[clipName];
    }
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
            
            if (!animator.IsInitialized()) continue;
            if (!animator.HasCurrentAnimation()) continue;
            if (animator.Paused) continue;
            
            animator.MSLeftToNextFrame -= deltaTimeMs;
            if (animator.MSLeftToNextFrame <= 0)
            {
                if (animator.ClipFrame < animator.Current().Frames.Length - 1)
                {
                    ref Sprite sprite = ref world.Pool<Sprite>().Get(animatedSprite);
                    animator.ClipFrame++;
                    animator.MSLeftToNextFrame = animator.Current().Frames[0].FrameDurationMS;
                    sprite.Texture = animator.Current().Frames[animator.ClipFrame].Texture;
                    sprite.Rect = animator.Current().Frames[animator.ClipFrame].Rect;
                }
                else if (animator.Current().Cycle)
                {
                    animator.ClipFrame = -1;
                }
            }
        }
    }
}