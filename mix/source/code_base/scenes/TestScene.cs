using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.imgui;
using destruct.scenes;
using mix.code_base.gameplay.systems;
using mix.gameplay.overlay;
using mix.gameplay.overlay.models;
using mix.gameplay.systems;
using mix.inputs;
using Raylib_cs;

namespace mix.code_base;

public sealed class TestScene : Scene, IDependenciesProvider
{
    private ECSWorld world;
    private Camera camera;
    
    private readonly UltimateInputService inputService = new();
    private TileFactory tileFactory;
    private GameMap gameMap;

    public override IDependenciesProvider[] GetProviders()
    {
        return [this];
    }

    protected override void OnSceneLoaded()
    {
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new SpriteRendererSystem(world), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new GameMapSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new HoverSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new SelectionSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CameraControlsSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new AnimationSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new HUDSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CellSelectionSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new TileSelectionSystem(), Systems));

        gameMap.Create(10,10);
        camera.CurrentCamera = new Camera2D();
        camera.CurrentCamera.Zoom = 2;

        world.BlockFiltersEvaluation();
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                tileFactory.CreateTileFromID(TileIds.AutumnTerrain, x, y);
            }
        }

        world.UnblockFiltersEvaluationAndEvaluate();

        tileFactory.CreateTileFromID(TileIds.Chainy, 1, 1);
        tileFactory.CreateTileFromID(TileIds.Ghost, 1, 2);
    }

    public override Task DisposeScene()
    {
        Assets.UnloadAll();
        return base.DisposeScene();
    }

    public override void Tick()
    {
        foreach (var inputsProcessor in Systems.Filter<ISceneSystemProcessInputs>())
        {
            inputsProcessor.ProcessInputs(deGUI.GUIWasOverlapped());
        }
        
        Raylib.BeginMode2D(camera.CurrentCamera);
        {
            foreach (var renderer in Systems.Filter<ISceneSystemWorldRenderer>())
            {
                renderer.RenderWorld();
            }
        }
        Raylib.EndMode2D();
        
        foreach (var postRenderer in Systems.Filter<ISceneSystemPostWorldRenderer>())
        {
            postRenderer.PostWorldRenderer();
        }
    }
    
    [Provide(true)]
    private ECSWorld ProvideECSWorldForDI()
    {
        world = new ECSWorld();
        return world;
    }
    
    [Provide(true)]
    private Camera ProvideCameraForDI()
    {
        camera = new Camera();
        return camera;
    }
    
    [Provide(true)]
    private ITileFactory ProvideTileFactoryForDI()
    {
        tileFactory = new TileFactory();
        return tileFactory;
    }
    
    [Provide(true)]
    private GameMap ProvideGameMapForDI()
    {
        gameMap = Game.G<DIKernel>().InjectWithBase(new GameMap());
        
        return gameMap;
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