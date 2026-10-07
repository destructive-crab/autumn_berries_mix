using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using mix.code_base;
using mix.gameplay.systems;

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
                s.Rotation = dir.Angle();
                LGR.PM(s.Rotation);
            });
        world.Pool<Animator>().New(entityId, (ref Animator a) =>
        {
            a.InitializeFromAse("assets/overlay/arrow.ase");
            a.Play("spawn");
        });
        world.Pool<Position>().New(entityId, (ref Position p) => p.Set(x, y, 1));
        world.Pool<Scale>().New(entityId, (ref Scale s) => s.Set(1,1));
        world.Pool<Origin>().New(entityId, (ref Origin o) => o.Set(0.5f, 0.5f));
        

        return entityId;
    }
}