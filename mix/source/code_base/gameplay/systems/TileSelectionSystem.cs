using System.Numerics;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;
using mix.code_base.gameplay.systems;
using mix.gameplay.systems;
using mix.inputs;

public sealed class TileSelectionSystem : ISceneSystem
{
    public string DebugName() => nameof(TileSelectionSystem);

    private Vec2I currentPointedCell;
    private Vec2I currentSelectedCell;
    
    private readonly Vector2 TileDetectionOffset;
    
    private ECSWorld      world;
    private GameMap map;
    [Inject] private IGridInputService inputService;
    
    //overlay
    private int pointedCellOverlayEntity;
    private int selectedCellOverlayEntity;

    private readonly Queue<int> selectionQueue = new();
    private int currentSelectedEntity = -1;
    
    public TileSelectionSystem()
    {
        TileDetectionOffset = new Vector2(Global.PixelsPerUnit/2, Global.PixelsPerUnit/2);
    }

    [Inject]
    private void Inject(ECSWorld world, GameMap map)
    {
        this.world = world;
        this.map = map;

        pointedCellOverlayEntity = world.New().Id;
        world.Pool<Position>().New(pointedCellOverlayEntity, (ref Position p) => p.Set(0,0,1));
        world.Pool<Origin>().New(pointedCellOverlayEntity, (ref Origin o) => o.Set(0.5f, 0.5f));
        world.Pool<Sprite>().New(pointedCellOverlayEntity, (ref Sprite s) =>
        {
            s = Sprite.From(AssetsPath.PointedCellBorder);
        });
        
        selectedCellOverlayEntity = world.New().Id;
        world.Pool<Position>().New(selectedCellOverlayEntity, (ref Position p) => p.Set(0,0,1));
        world.Pool<Origin>().New(selectedCellOverlayEntity, (ref Origin o) => o.Set(0.5f, 0.5f));
        world.Pool<Sprite>().New(selectedCellOverlayEntity, (ref Sprite s) =>
        {
            s = Sprite.From(AssetsPath.SelectedCellBorder);
            s.Hidden = true;
        });

        world
            .Filter()
            .In<Selected>()
            .Subscribe((h) =>
        {
            world.Pool<Sprite>().Get(selectedCellOverlayEntity).Hidden = h.Count() == 0;
            
            if (h.Count() == 0) return;
            
            world.Pool<Position>().Get(selectedCellOverlayEntity).X = world.Pool<Position>().Get(h.Last()).X;
            world.Pool<Position>().Get(selectedCellOverlayEntity).Y = world.Pool<Position>().Get(h.Last()).Y;
            currentSelectedEntity = h.Last();
        }).Evaluate();
        
        world
            .Filter()
            .In<Hovered>()
            .Subscribe((h) =>
        {
            if (h.Count() == 0) return;
            
            world.Pool<Sprite>().Get(pointedCellOverlayEntity).Hidden = h.Last() == currentSelectedEntity;
            world.Pool<Position>().Get(pointedCellOverlayEntity).X = world.Pool<Position>().Get(h.Last()).X;
            world.Pool<Position>().Get(pointedCellOverlayEntity).Y = world.Pool<Position>().Get(h.Last()).Y;
        }).Evaluate();
    }
}