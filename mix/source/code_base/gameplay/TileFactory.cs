using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct;
using destruct.gameplay.units;
using destruct.gameplay.units.ghost;
using destruct.tweens;
using mix.gameplay.overlay.units.abilities;
using Raylib_cs;

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
                Entity e = world.New()
                    .With(new Position(x, y, Const.TerrainZ))
                    .With(Origin.Center())
                    .With(new DrawOffset(0, 50))
                    .With(Scale.One())
                    .With(Sprite.From(SpriteAssets.AutumnTerrain))
                    .With<Tile>()
                    .With(new TileWalkSound("assets/audio/autumn_grass_tile_walk.ogg"))
                    .With(new Name("Autumn Terrain"))
                    .With(new Description("Walkable terrain"));

                return e.Id;
            }
            case TileIds.AutumnTree:
            {
                Entity e = world.New()
                    .With(new Position(x, y, Const.EntitiesZ))
                    .With(Origin.CenterBottom())
                    .With(new Rotation())
                    .With(Scale.One())
                    .With(Sprite.From(SpriteAssets.AutumnTree))
                    .With(new DrawOffset(0, 40))
                    .With(new Name("Autumn Tree"))
                    .With(new Description("Tree"))
                    .With<Tile>()
                    .With<BlockCell>()
                    .With(new SinScaleTween(Random.Shared.Next(70, 90) / 10f))
                    .With(new SinRotationTween(5, Random.Shared.Next(20, 30) / 10f));

                return e.Id;
            }
            case TileIds.Chainy:
            {
                Entity tileEntity = world.New();
                
                tileEntity
                    .With(new Position(x, y, Const.EntitiesZ))
                    .With(Origin.CenterBottom())
                    .With(Scale.One())
                    .With<Rotation>()
                    .With(new SinScaleTween(3))
                    .With(Sprite.From(SpriteAssets.Chainy))
                    .With<Tile>()
                    .With<RendererColor>(new RendererColor(Color.White))
                    .With<BlockCell>()
                    .With<HitSound>(new HitSound(SFX.ChainyHit, 1))
                    .With(new Health(5,5))
                    .With(new Name("Chainy"))
                    .With(new Description("chain chain chain swag"))
                    .With((ref PlayerUnit u) =>
                         {
                             Entity moveAbility = world.New()
                                 .With(new PlayerUnitAbility(tileEntity.Id, "assets/units/player/move_ability.png"))
                                 .With(new MoveAbility());
                             
                             Entity chainsawAttack = world.New()
                                 .With(new PlayerUnitAbility(tileEntity.Id, "assets/units/player/chainsaw_attack_ability.png"))
                                 .With(new BasicAttackAbility(1, SFX.Chainsaw));
                             
                             u.Default();
                             u.Abilities = [ moveAbility.Id, chainsawAttack.Id ];
                         });
                
                /*
                world.Pool<SpriteAnimation>().New(tileEntity, (ref SpriteAnimation animator) =>
                {
                    //animator.InitializeFromAse("assets/units/player/chainy.ase");
                    //animator.Play("idle");
                });
                
                tileEntity.With(new Animator("assets/units/player/chainy_by_mivioon.ase", 
                    ["move"],
                    new AnimatorState(["hit"], [], "hit"),
                    new AnimatorState(["basic_attack"], [], "basic_attack"),
                    new AnimatorState([], [new("move", true)], "move"),
                    new AnimatorState("idle")
                ));
                */
                return tileEntity.Id;
            }
            case TileIds.Ghost:
            {
                Entity tileEntity = world.New();

                tileEntity
                    .With(new Position(x, y, Const.EntitiesZ))
                    .With(new Origin(0.5f, 1f))
                    .With(new RendererColor(Color.White))
                    .With(Scale.One())
                    .With<Rotation>()
                    .With(new SinScaleTween(3))
                    .With(Sprite.From(SpriteAssets.Ghost))
                    .With<Tile>()
                    .With<EnemyUnit>()
                    .With<Ghost>(new Ghost(1))
                    .With<HitSound>(new HitSound(SFX.GhostHit, 0.8f))
                    .With<BlockCell>()
                    .With(new Health(5, 5))
                    .With(new Name("Ghost"))
                    .With(new Description("oOooOooooooO"));
                
                return tileEntity.Id;
            }
        }

        throw new Exception($"Factory has no initializer for {id}");
    }
}