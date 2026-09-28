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
                world.Pool<Position>().NewComponent(tileId, (ref Position p) => p.Set(x,y) );
                world.Pool<Sprite>()  .NewComponent(tileId, (ref Sprite s) =>
                {
                    s = Sprite.From("assets/tiles/autumn_terrain.png");
                    s.DrawOffsetInPixels = new(0, 3);
                });
                world.Pool<Tile>().NewComponent(tileId);
                world.Pool<Name>().NewComponent(tileId, (ref Name name) => name.Value = "Autumn Terrain");
                world.Pool<Description>().NewComponent(tileId, (ref Description description) => description.Value = "Walkable terrain");
            }
        }
        world.UnblockFiltersEvaluationAndEvaluate();

        var chainyId = world.New();
        world.Pool<Position>().NewComponent(chainyId, (ref Position p) => p.Set(1,1));
        world.Pool<Sprite>().NewComponent(chainyId, (ref Sprite s) =>
        {
            s = Sprite.From("assets/units/player/chainy.png");
            s.DrawOrder = 3;
        });
        world.Pool<Tile>().NewComponent(chainyId);
        
        var ghostId = world.New();
        world.Pool<Position>().NewComponent(ghostId, (ref Position p) => p.Set(1,2));
        world.Pool<Sprite>().NewComponent(ghostId, (ref Sprite s) =>
        {
            s = Sprite.From("assets/units/enemies/ghost.png");
            s.DrawOrder = 3;
        });
        world.Pool<Tile>().NewComponent(ghostId);
        world.Pool<Name>().NewComponent(ghostId, (ref Name name) => name.Value = "Ghost!!!");
        world.Pool<Description>().NewComponent(ghostId, (ref Description description) => description.Value = "!!!!!!!");
        world.Pool<Animator>().NewComponent(ghostId, (ref Animator animator) =>
        {
            var t = Assets.LoadTexture("assets/units/enemies/ghost.png");
            animator.ClipFrame = 0;
            AnimationClip clip = new();
            clip.Cycle = true;
            
            AnimationClipFrame frame1 = new(t, 0,0,36,36);
            AnimationClipFrame frame2 = new(t, 36,0,36,36);
            AnimationClipFrame frame3 = new(t, 72,0,36,36);
            AnimationClipFrame frame4 = new(t, 108,0,36,36);

            clip.Frames = [frame1, frame2, frame3, frame4];

            animator.Current = clip;
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