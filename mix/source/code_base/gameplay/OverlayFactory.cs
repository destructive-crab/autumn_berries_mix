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

    public List<int> SpawnArrows(IEnumerable<Vec2I> positions, Vec2I origin)
    {
        List<int> overlayEntityIds = new();
        foreach (Vec2I position in positions)
        {
            overlayEntityIds.Add(SpawnArrow(position.X, position.Y, position - origin));
        }

        return overlayEntityIds;
    }

    public int SpawnArrow(int x, int y, Vec2I dir)
    {
        int entityId = world.New().Id;
        world.Pool<Sprite>().New(entityId,
            (ref Sprite s) =>
            {
                s.Rotation = dir.Angle();
            });
        world.Pool<SpriteAnimation>().New(entityId, (ref SpriteAnimation a) =>
        {
        });
        //world.Pool<Animator>().New(entityId, (ref Animator a) =>
        //{
        //    a.WithAseAnimations("assets/overlay/arrow.ase");
        //    AnimatorState state = AnimatorUTL.State("spawn");
        //    state.NewTransition().TransitionDestination("idle");
        //    a.AddState()
        //    a.HasTrigger("spawn");
        //});
        world.Pool<Position>().New(entityId, (ref Position p) => p.Set(x, y, 1));
        world.Pool<Scale>().New(entityId, (ref Scale s) => s.Set(1,1));
        world.Pool<Origin>().New(entityId, (ref Origin o) => o.Set(0.5f, 0.5f));
        world.Pool<Hoverable>().New(entityId);
        world.Pool<CursorAnimationReaction>().New(entityId, (ref CursorAnimationReaction r) =>
        {
            r.Idle = "idle";
            r.Hovered = "hovered";
        });

        return entityId;
    }

    public int[] SpawnAttackPointers(IEnumerable<Vec2I> positions)
    {
        int i = 0;
        int[] result = new int[positions.Count()];
        foreach (Vec2I position in positions)
        {
            result[i] = SpawnAttackPointer(position.X, position.Y);
        }

        return result;
    }

    public int SpawnAttackPointer(int x, int y)
    {
        return world.New()
            .With<Position>(new Position(x, y - 0.5f, 3))
            .With<Scale>(new Scale(1, 1))
            .With<Origin>(new Origin(0.5f, 0.5f))
            .With<Hoverable>()
            .With<Sprite>(Sprite.From("assets/overlay/attack_pointer.png"))
            .Id;
    }
}