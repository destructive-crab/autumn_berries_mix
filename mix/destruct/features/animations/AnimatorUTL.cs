using destruct.Debug;
using destruct.ecs;

namespace mix.gameplay.systems;

public static class AnimatorUTL
{
    public static bool HasAnimation(ECSWorld world, int entity, string trackName)
    {
        //return world.HasIn<Animator>(entity) && world.HasIn<SpriteAnimation>(entity) && world.GetFrom<Animator>(entity).Initialized() && world.GetFrom<Animator>(entity).Container.Contains(trackName);
        return false;
    }
    
    public static void Play(ECSWorld world, int entity, string trackName)
    {
        if (!HasAnimation(world, entity, trackName))
        {
            LGR.PERR($"No animation {trackName} found on {entity}");
        }

        //world.GetFrom<SpriteAnimation>(entity).SetClip(world.GetFrom<Animator>(entity).Container.Get(trackName));
    }
}

public static class EventUtils
{
    public static bool TryPushEventToEntity(ECSWorld world, int entity, object newEvent)
    {
        if (!world.HasIn<EventsRoot>(entity)) return false;

        world.GetFrom<EventsRoot>(entity).NewEvent = newEvent;

        return true;
    }
}