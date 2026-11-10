using destruct.Debug;

namespace mix.gameplay.systems;

public sealed class AnimationsContainer : IDisposable
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
            return new();
        }

        return clips[clipName];
    }

    public void Dispose() { }
}