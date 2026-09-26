using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base.gameplay;
using mix.code_base.gameplay.systems;
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
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new RendererSystem(world), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new GameMapSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new TileSelectionSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CameraControlsSystem(), Systems));

        camera.CurrentCamera = new Camera2D();
        camera.CurrentCamera.Zoom = 2;

        world.BlockFiltersEvaluation();
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                int tileId = world.New().Id;
                world.Pool<Position>().NewComponent(tileId, (ref Position p) => p.Set(x,y) );
                world.Pool<Sprite>()  .NewComponent(tileId, (ref Sprite t) =>
                {
                    t.Texture = Assets.LoadTexture("assets/tiles/autumn_terrain.png");
                    t.Pivot = new(0.5f, 0.5f);
                    t.DrawOffsetInPixels = new(0, 3);
                    t.DrawOrder = 0;
                });
                world.Pool<Tile>()    .NewComponent(tileId);
                world.Pool<Name>().NewComponent(tileId, (ref Name name) => name.Value = "Autumn Terrain");
                world.Pool<Description>().NewComponent(tileId, (ref Description description) => description.Value = "Walkable terrain");
            }
        }
        world.UnblockFiltersEvaluationAndEvaluate();

        var chainyId = world.New();
        world.Pool<Position>().NewComponent(chainyId, (ref Position p) => p.Set(1,1));
        world.Pool<Sprite>().NewComponent(chainyId, (ref Sprite t) =>
        {
            t.Texture = Assets.LoadTexture("assets/units/player/chainy.png");
            t.Pivot = new(0.5f, 0f);
            t.DrawOrder = 3;
        });
        world.Pool<Tile>().NewComponent(chainyId);
        
        var ghostId = world.New();
        world.Pool<Position>().NewComponent(ghostId, (ref Position p) => p.Set(1,2));
        world.Pool<Sprite>().NewComponent(ghostId, (ref Sprite t) =>
        {
            t.Texture = Assets.LoadTexture("assets/units/enemies/ghost.png");
            t.Pivot = new(0.5f, 0f);
            t.DrawOrder = 3;
        });
        world.Pool<Tile>().NewComponent(ghostId);
        world.Pool<Name>().NewComponent(ghostId, (ref Name name) => name.Value = "Ghost!!!");
        world.Pool<Description>().NewComponent(ghostId, (ref Description description) => description.Value = "!!!!!!!");
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