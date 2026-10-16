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

public sealed class TestScene : Scene, IDependenciesProvider
{
    private ECSWorld world;
    private Camera camera;
    
    private readonly UltimateInputService inputService = new();
    private TileFactory tileFactory;
    private GameMap gameMap;

    private Shader backgroundShader;
    private int timeLoc;
    private TurnState turnState;

    public override IDependenciesProvider[] GetProviders()
    {
        return [this];
    }
    
    protected override void OnSceneLoaded()
    {
        backgroundShader = Raylib.LoadShader(null, "assets/shaders/background.fs");
        timeLoc = Raylib.GetShaderLocation(backgroundShader, "time");
        
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new SpriteRendererSystem(world), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new GameMapSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new HoverSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new SelectionSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CameraControlsSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new AnimationSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CellSelectionSystemsRoot(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new CellSelectionRedirector(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new AnimationCursorReactionSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new MoveAbilitySystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new PositionTweenSystem(), Systems));
        Systems.Register(Game.G<DIKernel>().InjectWithBaseAnd(new HUDSystem(), Systems));

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
        
        turnState.StartTurnCycle(new PlayerTurn());
    }

    public override Task DisposeScene()
    {
        Assets.UnloadAll();
        return base.DisposeScene();
    }

    public override void Tick()
    {
        DrawBackgroundShader();

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

    private void DrawBackgroundShader()
    {
        float time = (float)Raylib.GetTime();
        Raylib.SetShaderValue(backgroundShader, timeLoc, time, ShaderUniformDataType.Float);
        Raylib.BeginShaderMode(backgroundShader);
        {
            Rlgl.Begin(DrawMode.Triangles);
            {
                Rlgl.TexCoord2f(0.0f, 0.0f);
                Rlgl.Vertex2f(0.0f, 0.0f);
        
                Rlgl.TexCoord2f(0.0f, Raylib.GetScreenHeight());
                Rlgl.Vertex2f(0.0f, Raylib.GetScreenHeight());
        
                Rlgl.TexCoord2f(Raylib.GetScreenWidth(), 0.0f);
                Rlgl.Vertex2f(Raylib.GetScreenWidth(), 0.0f);
        
                Rlgl.TexCoord2f(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                Rlgl.Vertex2f(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        
                Rlgl.TexCoord2f(Raylib.GetScreenWidth(), 0.0f);
                Rlgl.Vertex2f(Raylib.GetScreenWidth(), 0.0f);
        
                Rlgl.TexCoord2f(0.0f, Raylib.GetScreenHeight());
                Rlgl.Vertex2f(0.0f, Raylib.GetScreenHeight());
            }
            Rlgl.End();
        }
        Raylib.EndShaderMode();
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