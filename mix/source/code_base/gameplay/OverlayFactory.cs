using destruct.DI;
using destruct.ecs;
using mix.code_base;

namespace mix.gameplay.overlay;

public struct OverlayAssetPaths
{
    public const string Arrow = "assets/overlay/arrow.png";
}

public struct CursorAnimationReaction
{
    public bool StopReactions;
    
    public bool PauseOnIdle;
    
    public string Idle;
    public string Hovered;
    public string Pressed;
}

public sealed class OverlayFactory
{
    [Inject] private ECSWorld world;

    public int SpawnArrow(int x, int y, Vec2I dir)
    {
        int entityId = world.New().Id;
        world.Pool<Sprite>().New(entityId,
            (ref Sprite s) =>
            {
                s = Sprite.From(OverlayAssetPaths.Arrow);
            });
        
                //s.Rotation = (float)Math.Atan2(dir.Y, dir.X);

        return entityId;
    }
}