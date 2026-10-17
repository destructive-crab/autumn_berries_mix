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
        animator.ClipFrame = -1;
        animator.TimeLeftToNextFrame = 0;
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
    public TextureFragment Fragment;

    public int FrameDurationMS;

    public AnimationClipFrame() {}
    
    public AnimationClipFrame(CachedTexture texture, int frameDurationMs, Rectangle rect)
    {
        Fragment = new TextureFragment(texture, rect);
        FrameDurationMS = frameDurationMs;
    }

    public AnimationClipFrame(CachedTexture texture, int x, int y, int w, int h)
    {
        FrameDurationMS = 200;
        Fragment = new(texture, new(x,y,w,h));
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
    public float TimeLeftToNextFrame;
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

public sealed class AnimationSystem : ISystem, IWorldRendererSystem
{
    public string DebugName() => nameof(AnimationSystem);

    private ECSWorld world;
    private EntityComponentPool<Animator> animatorsPool;
    
    private IEnumerable<int> animatedSprites = [];

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

    public void RenderWorld()
    {
        float deltaTimeMs = Game.DeltaTime * 1000;
        
        foreach (int animatedSprite in animatedSprites)
        {
            ref Animator animator = ref animatorsPool.Get(animatedSprite);
            
            if (!animator.IsInitialized()) continue;
            if (!animator.HasCurrentAnimation()) continue;
            if (animator.Paused) continue;
            
            animator.TimeLeftToNextFrame -= deltaTimeMs;
            if (animator.TimeLeftToNextFrame <= 0)
            {
                if (animator.ClipFrame < animator.Current().Frames.Length - 1)
                {
                    ref Sprite sprite = ref world.Pool<Sprite>().Get(animatedSprite);
                    animator.ClipFrame++;
                    animator.TimeLeftToNextFrame = animator.Current().Frames[0].FrameDurationMS;
                    sprite.Fragment = animator.Current().Frames[animator.ClipFrame].Fragment;
                }
                else if (animator.Current().Cycle)
                {
                    animator.ClipFrame = -1;
                }
            }
        }
    }
}