namespace mix.gameplay.systems;

public struct SpriteAnimation
{
    public bool Paused;
    
    public AnimationClip Clip;
    
    public int ClipFrame;
    
    public float TimeLeftToNextFrame;

    public bool IsEmpty() => string.IsNullOrEmpty(Clip.Name);
}