using System.Numerics;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;
using mix.code_base.gameplay.systems;
using mix.gameplay.systems;
using mix.inputs;

public sealed class CellSelectionSystem : ISceneSystem
{
    public string DebugName() => nameof(CellSelectionSystem);

    private Vec2I currentPointedCell;
    private Vec2I currentSelectedCell;
    
    private readonly Vector2 TileDetectionOffset;
    
    private ECSWorld world;
    private GameMap map;
    
    [Inject] private IGridInputService inputService;
    
    //overlay
    private int pointedCellOverlayEntity;
    private int selectedCellOverlayEntity;

    private readonly Queue<int> selectionQueue = new();
    private int currentSelectedEntity = -1;
    
    public CellSelectionSystem()
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
        
        world
            .Filter()
            .In<Selected>()
            .In<Cell>()
            .Subscribe((e) =>
        {
            if (e.Count() == 0) return;
            
            int lastSelected = e.Last();

            int topEntity = world.Pool<Cell>().Get(lastSelected).Entities[^1];

            if (currentSelectedEntity != -1)
            {
                world.Pool<Selected>().Remove(currentSelectedEntity);
            }
            
            world.Pool<Selected>().New(topEntity);
            
            currentSelectedEntity = topEntity;
        }).Evaluate();
        
        world
            .Filter()
            .In<Hovered>()
            .Subscribe((e) =>
        {
            if (e.Count() == 0) return;
            
            world.Pool<Sprite>().Get(pointedCellOverlayEntity).Hidden = e.Last() == currentSelectedEntity;
            world.Pool<Position>().Get(pointedCellOverlayEntity).X = world.Pool<Position>().Get(e.Last()).X;
            world.Pool<Position>().Get(pointedCellOverlayEntity).Y = world.Pool<Position>().Get(e.Last()).Y;
        }).Evaluate();
    }
}