using destruct.assets;
using Raylib_cs;

namespace destruct.postprocessing;

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