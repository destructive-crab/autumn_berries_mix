using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.imgui;
using destruct.scenes;
using mix.code_base.gameplay.systems;
using mix.gameplay.overlay;
using mix.gameplay.overlay.models;
using mix.gameplay.overlay.tweens;
using mix.gameplay.overlay.units.abilities;
using mix.gameplay.systems;
using mix.inputs;
using Raylib_cs;

namespace mix.code_base;

public abstract class ECSScene : Scene
{
    public readonly ECSWorld World = new();
}

public sealed class TestScene : ECSScene, IDependenciesProvider
{
    private Camera camera;
    
    private readonly UltimateInputService inputService = new();
    private TileFactory tileFactory;
    private GameMap gameMap;

    private TurnState turnState;

    public override IDependenciesProvider[] GetProviders()
    {
        return [this];
    }
    
    protected override void OnSceneLoaded()
    {
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new BackgroundSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new SpriteRendererSystem(World), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new SinScaleTweenSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new SinRotationTweenSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new GameMapSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new HoverSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new SelectionSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CameraControlsSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new AnimatorSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new AnimationSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CellSelectionSystemsRoot(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CellSelectionRedirector(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new AnimationCursorReactionSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new MoveAbilitySystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new BasicAttackSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new PositionTweenSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CursorSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new SelectionSoundSystem(), Systems));
        
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new HUDSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new DebugHUDSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new ConsoleSystem(), Systems));

        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new HealthSystem(), Systems));
       
        gameMap.Create(10,10);
        camera.CurrentCamera = new Camera2D();
        camera.CurrentCamera.Zoom = 0.5f;

        World.BlockFiltersEvaluation();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                tileFactory.CreateTileFromID(TileIds.AutumnTerrain, x, y);
                if ((x != 1 && y != 1) && (x == 0 || y == 0 || y == 3 || x == 7 || Random.Shared.Next(0, 3) == 1))
                {
                    tileFactory.CreateTileFromID(TileIds.AutumnTree, x, y);
                }
            }
        }

        World.UnblockFiltersEvaluationAndEvaluate();

        tileFactory.CreateTileFromID(TileIds.Chainy, 1, 1);
        //tileFactory.CreateTileFromID(TileIds.Ghost, 1, 2);
        
        turnState.StartTurnCycle(new PlayerTurn());
        
        Game.AudioService.PlayMusic("assets/audio/main_theme.ogg", 40);
    }

    public override Task DisposeScene()
    {
        Assets.UnloadAll();
        return base.DisposeScene();
    }

    public override void Tick()
    {
        foreach (var inputsProcessor in Systems.Filter<IInputsProcessSystem>())
        {
            inputsProcessor.ProcessInputs(deGUI.GUIWasOverlapped());
        }
        
        foreach (var gameLogic in Systems.Filter<IGameLogicSystem>())
            gameLogic.Process();
        
        foreach (var preWorldRenderer in Systems.Filter<IPreWorldRendererSystem>())
            preWorldRenderer.PreWorldRenderer();
        
        Raylib.BeginMode2D(camera.CurrentCamera);
        {
            foreach (var renderer in Systems.Filter<IWorldRendererSystem>())
                renderer.RenderWorld();
        }
        Raylib.EndMode2D();
        
        foreach (var postRenderer in Systems.Filter<IPostWorldRendererSystem>())
        {
            postRenderer.PostWorldRenderer();
        }
        
        deGUI.BeginGUI();
        {
            foreach (var guiSystem in Systems.Filter<IGUISystem>())
            {
                guiSystem.DrawGUI();
            }
        }
        deGUI.EndGUI();
    }

    [Provide(true)]
    private ECSWorld ProvideECSWorldForDI() => World;

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
    private TurnState ProvideTurnStateForDI()
    {
        turnState = new TurnState();
        return turnState;
    }

    [Provide(true)]
    private IGeneralInputService ProvideGeneralInputServiceForDI() => inputService;

    [Provide(true)]
    private OverlayFactory ProvideOverlayFactoryForDI() => new();

    [Provide(true)]
    private IGridInputService ProvideGridInputServiceForDI() => inputService;
    
    [Provide(true)]
    private IGameplayInputService ProvideGameplayInputServiceForDI() => inputService;
    
    [Provide(true)]
    private ICameraInputSystem ProvideCameraMovementInputServiceForDI() => inputService;
}