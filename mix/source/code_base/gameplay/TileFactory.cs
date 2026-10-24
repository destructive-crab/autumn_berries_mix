using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using mix.code_base;
using mix.gameplay.overlay.units.abilities;
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
                world.Pool<TileWalkSound>().New(tileId, (ref TileWalkSound s) => s.SoundAssetPath = "assets/audio/autumn_grass_tile_walk.ogg");
                
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
                var tileEntity= world.New();
                world.Pool<Position>().New(tileEntity, (ref Position p) => p.Set(x, y, 2));
                world.Pool<Origin>().New(tileEntity, (ref Origin o) => o.Set(0.5f,0.8f));
                world.Pool<Scale>().New(tileEntity, (ref Scale s) => s.Set(1,1));
                world.Pool<Sprite>().New(tileEntity, (ref Sprite s) =>
                {
                    s = Sprite.From("assets/units/player/chainy.png");
                });
                world.Pool<Tile>().New(tileEntity);
                world.Pool<BlockCell>().New(tileEntity);
                
                world.Pool<SpriteAnimation>().New(tileEntity, (ref SpriteAnimation animator) =>
                {
                    //animator.InitializeFromAse("assets/units/player/chainy.ase");
                    //animator.Play("idle");
                });
                
                tileEntity.With(new Animator("assets/units/player/chainy.ase", 
                    ["move"],
                    new AnimatorState(["hit"], [], "hit"),
                    new AnimatorState(["basic_attack"], [], "basic_attack"),
                    new AnimatorState([], [new("move", true)], "move"),
                    new AnimatorState("idle")
                ));
                
                world.Pool<Name>()
                    .New(tileEntity, (ref Name name) => name.Value = "Chainy");
                world.Pool<Description>()
                    .New(tileEntity, (ref Description description) => description.Value = "text text text text text text sdkfsjdflksjdf text fkdsjf text dskfk text tkdkfj");

                world.Pool<Health>().New(tileEntity, (ref Health h) => h.Set(5,5));
                
                world.Pool<PlayerUnit>().New(tileEntity, (ref PlayerUnit u) =>
                {
                    Entity moveAbility = world.New()
                        .With(new PlayerUnitAbility(tileEntity.Id, "assets/units/player/move_ability.png"))
                        .With(new MoveAbility());

                    Entity chainsawAttack = world.New()
                        .With(new PlayerUnitAbility(tileEntity.Id, "assets/units/player/chainsaw_attack_ability.png"))
                        .With(new BasicAttackAbility());
                    
                    u.Default();
                    u.Abilities = [ moveAbility.Id, chainsawAttack.Id ];
                });
                return tileEntity.Id;
            }
            case TileIds.Ghost:
            {
                var tileEntity = world.New();
                world.Pool<Position>().New(tileEntity, (ref Position p) => p.Set(x, y, 2));
                world.Pool<Origin>().New(tileEntity, (ref Origin o) => o.Set(0.5f,1f));
                world.Pool<Scale>().New(tileEntity, (ref Scale s) => s.Set(1,1));
                world.Pool<Sprite>().New(tileEntity);
                
                tileEntity.With(new Animator("assets/units/enemies/ghost.ase", 
                    ["move"],
                    new AnimatorState(["hit"], [], "hit"),
                    new AnimatorState(["basic_attack"], [], "basic_attack"),
                    new AnimatorState([], [new("move", true)], "move"),
                    new AnimatorState("idle")
                ));
                
                world.Pool<Tile>().New(tileEntity);
                world.Pool<Health>().New(tileEntity, (ref Health h) => h.Set(5,5));
                world.Pool<Name>().New(tileEntity, (ref Name name) => name.Value = "Ghost!!!");
                world.Pool<BlockCell>().New(tileEntity);
                world.Pool<Description>().New(tileEntity, (ref Description description) => description.Value = "!!!!!!!");
                world.Pool<SpriteAnimation>().New(tileEntity, (ref SpriteAnimation animator) =>
                {
                    //animator.InitializeFromAse("assets/units/enemies/ghost.ase");
                    //animator.Play("idle");
                });

                return tileEntity.Id;
            }
        }

        throw new Exception($"Factory has no initializer for {id}");
    }
}