using destruct.ecs;
using destruct.tweens;
using Raylib_cs;

namespace destruct;

public static class HealthUTL
{
    public static void Hit(ECSWorld world, int entity, int damage)
    {
        world.GetFrom<Health>(entity).Current -= damage;
        Tweens.PingPongColorTween(world, entity, new(255, 0, 0, 200), 1200);


        if (world.HasIn<HitSound>(entity))
        {
            var sound = world.GetFrom<HitSound>(entity);
            Game.AudioService.PlaySound(sound.AssetPath, sound.Volume, UTL.RandomPitchForSound());
        }
    }
}

public struct HitSound
{
    public string AssetPath;
    public float  Volume;

    public HitSound(string assetPath, float volume)
    {
        AssetPath = assetPath;
        Volume = volume;
    }
}