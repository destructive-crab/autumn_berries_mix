using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base.gameplay;
using mix.gameplay.systems;
using mix.inputs;
using Raylib_cs;

namespace mix.code_base;

public sealed class TestScene : Scene, IDependenciesProvider
{
    private ECSWorld world;
    private Camera camera;
    private readonly UltimateInputService inputService = new();
    
    public override IDependenciesProvider[] GetProviders()
    {
        return [this];
    }

    protected override void OnSceneLoaded()
    {
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new SpriteRendererSystem(world), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new GameMapSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new TileSelectionSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CameraControlsSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new AnimationSystem(), Systems));

        camera.CurrentCamera = new Camera2D();
        camera.CurrentCamera.Zoom = 2;

        world.BlockFiltersEvaluation();
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                int tileId = world.New().Id;
                world.Pool<Position>().New(tileId, (ref Position p) => p.Set(x,y) );
                world.Pool<Sprite>().New(tileId, (ref Sprite s) =>
                {
                    s = Sprite.From("assets/tiles/autumn_terrain.png");
                    s.DrawOffsetInPixels = new(0, 3);
                });
                world.Pool<Tile>().New(tileId);
                world.Pool<Name>().New(tileId, (ref Name name) => name.Value = "Autumn Terrain");
                world.Pool<Description>().New(tileId, (ref Description description) => description.Value = "Walkable terrain");
            }
        }
        world.UnblockFiltersEvaluationAndEvaluate();

        var chainyId = world.New();
        world.Pool<Position>().New(chainyId, (ref Position p) => p.Set(1,1));
        world.Pool<Sprite>().New(chainyId, (ref Sprite s) =>
        {
            s = Sprite.From("assets/units/player/chainy.png");
            s.DrawOrder = 3;
        });
        world.Pool<Tile>().New(chainyId);
        world.Pool<Animator>().New(chainyId, (ref Animator animator) =>
        {
            animator.InitializeFromAse("assets/units/player/chainy.ase");
            animator.Play("idle");
        });
        
        var ghostId = world.New();
        world.Pool<Position>().New(ghostId, (ref Position p) => p.Set(1,2));
        world.Pool<Sprite>().New(ghostId, (ref Sprite s) =>
        {
            s = Sprite.From("assets/units/enemies/ghost.png");
            s.DrawOrder = 3;
        });
        world.Pool<Tile>().New(ghostId);
        world.Pool<Name>().New(ghostId, (ref Name name) => name.Value = "Ghost!!!");
        world.Pool<Description>().New(ghostId, (ref Description description) => description.Value = "!!!!!!!");
        world.Pool<Animator>().New(ghostId, (ref Animator animator) =>
        {
            animator.InitializeFromAse("assets/units/enemies/ghost.ase");
            animator.Play("idle");
        });
    }

    public override Task DisposeScene()
    {
        Assets.UnloadAll();
        return base.DisposeScene();
    }

    public override void Tick()
    {
        Raylib.BeginMode2D(camera.CurrentCamera);
        {
            foreach (ISceneSystemTick tick in Systems.Filter<ISceneSystemTick>())
            {
                tick.Tick();
            }
        }
        Raylib.EndMode2D();
    }
    
    [Provide(true)]
    private ECSWorld ProvideECSWorldForDI()
    {
        world = new();
        return world;
    }
    
    [Provide(true)]
    private Camera ProvideCameraForDI()
    {
        camera = new();
        return camera;
    }
    
    [Provide(true)]
    private IGeneralInputService ProvideGeneralInputServiceForDI() => inputService;
    [Provide(true)]
    private IGridInputService ProvideGridInputServiceForDI() => inputService;
    [Provide(true)]
    private IGameplayInputService ProvideGameplayInputServiceForDI() => inputService;
    [Provide(true)]
    private ICameraInputSystem ProvideCameraMovementInputServiceForDI() => inputService;
}