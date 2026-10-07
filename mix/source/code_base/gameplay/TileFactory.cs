using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using mix.code_base;
using mix.gameplay.overlay.units;
using mix.gameplay.systems;

namespace mix.gameplay.overlay;

public struct TileIds
{
    //environment
    public const string AutumnTerrain = "autumn_terrain";
    public const string AutumnTree = "autumn_tree";

    //player units
    public const string Chainy = "chainy";

    //enemy units
    public const string Ghost = "ghost";
}

public interface ITileFactory
{
    int CreateTileFromID(string id, int x, int y);
}

public sealed class TileFactory : ITileFactory
{
    [Inject] private ECSWorld world;

    public int CreateTileFromID(string id, int x, int y)
    {
        switch (id)
        {
            case TileIds.AutumnTerrain:
            {
                int tileId = world.New().Id;
                world.Pool<Position>().New(tileId, (ref Position p) => p.Set(x, y));
                world.Pool<Origin>().New(tileId, (ref Origin o) => o.Set(0.5f,0.5f));
                world.Pool<Scale>().New(tileId, (ref Scale s) => s.Set(1,1));
                world.Pool<Sprite>().New(tileId, (ref Sprite s) =>
                {
                    s = Sprite.From("assets/tiles/autumn_terrain.png");
                    s.DrawOffsetInPixels = new(0, 3);
                });
                world.Pool<Tile>().New(tileId);
                
                world.Pool<Name>()
                    .New(tileId, (ref Name name) => name.Value = "Autumn Terrain");
                world.Pool<Description>()
                    .New(tileId, (ref Description description) => description.Value = "Walkable terrain");

                return tileId;
            }
            case TileIds.AutumnTree:
            {
                int tileId = world.New().Id;
                world.Pool<Position>().New(tileId, (ref Position p) => p.Set(x, y));
                world.Pool<Origin>().New(tileId, (ref Origin o) => o.Set(0.5f,0.5f));
                world.Pool<Scale>().New(tileId, (ref Scale s) => s.Set(1,1));
                world.Pool<Sprite>().New(tileId,
                    (ref Sprite s) => { s = Sprite.From("assets/tiles/autumn_tree.png"); });
                world.Pool<Tile>().New(tileId);
                world.Pool<Name>().New(tileId, (ref Name name) => name.Value = "Autumn Terrain");
                world.Pool<Description>().New(tileId,
                    (ref Description description) => description.Value = "Walkable terrain");

                return tileId;
            }
            case TileIds.Chainy:
            {
                var tileId= world.New();
                world.Pool<Position>().New(tileId, (ref Position p) => p.Set(x, y, 2));
                world.Pool<Origin>().New(tileId, (ref Origin o) => o.Set(0.5f,0.8f));
                world.Pool<Scale>().New(tileId, (ref Scale s) => s.Set(1,1));
                world.Pool<Sprite>().New(tileId, (ref Sprite s) =>
                {
                    s = Sprite.From("assets/units/player/chainy.png");
                });
                world.Pool<Tile>().New(tileId);
                
                world.Pool<Animator>().New(tileId, (ref Animator animator) =>
                {
                    animator.InitializeFromAse("assets/units/player/chainy.ase");
                    animator.Play("idle");
                });
                
                world.Pool<Name>()
                    .New(tileId, (ref Name name) => name.Value = "Chainy");
                world.Pool<Description>()
                    .New(tileId, (ref Description description) => description.Value = "text text text text text text sdkfsjdflksjdf text fkdsjf text dskfk text tkdkfj");

                world.Pool<PlayerUnitAbilities>().New(tileId, 
                    (ref PlayerUnitAbilities a) => a.Abilities = [new MoveAbility(tileId.Id)]);
                return tileId.Id;
            }
            case TileIds.Ghost:
            {
                var tileId = world.New();
                world.Pool<Position>().New(tileId, (ref Position p) => p.Set(x, y, 2));
                world.Pool<Origin>().New(tileId, (ref Origin o) => o.Set(0.5f,1f));
                world.Pool<Scale>().New(tileId, (ref Scale s) => s.Set(1,1));
                world.Pool<Sprite>().New(tileId, (ref Sprite s) =>
                {
                    s = Sprite.From("assets/units/enemies/ghost.png");
                });
                world.Pool<Tile>().New(tileId);
                world.Pool<Name>().New(tileId, (ref Name name) => name.Value = "Ghost!!!");
                world.Pool<Description>().New(tileId, (ref Description description) => description.Value = "!!!!!!!");
                world.Pool<Animator>().New(tileId, (ref Animator animator) =>
                {
                    animator.InitializeFromAse("assets/units/enemies/ghost.ase");
                    animator.Play("idle");
                });

                return tileId.Id;
            }
        }

        throw new Exception($"Factory has no initializer for {id}");
    }
}