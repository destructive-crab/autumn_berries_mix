using System.Numerics;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;
using mix.inputs;

public sealed class TileSelectionSystem : ISceneSystem, ISceneSystemTick
{
    public string DebugName() => nameof(TileSelectionSystem);

    private Vec2I currentPointedCell;
    private Vec2I currentSelectedCell;
    
    private readonly Vector2 TileDetectionOffset;
    
    private ECSWorld      world;
    private GameMapSystem map;
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
    private void Inject(ECSWorld world, GameMapSystem map)
    {
        this.world = world;
        this.map = map;

        pointedCellOverlayEntity = world.New().Id;
        world.Pool<Position>().NewComponent(pointedCellOverlayEntity);
        world.Pool<Sprite>().NewComponent(pointedCellOverlayEntity, (ref Sprite s) =>
        {
            s = Sprite.From(AssetsPath.PointedCellBorder);
            s.DrawOrder = 1;
        });
        
        selectedCellOverlayEntity = world.New().Id;
        world.Pool<Position>().NewComponent(selectedCellOverlayEntity);
        world.Pool<Sprite>().NewComponent(selectedCellOverlayEntity, (ref Sprite s) =>
        {
            s = Sprite.From(AssetsPath.SelectedCellBorder);
            s.DrawOrder = 1;
            s.Hidden = true;
        });
    }
    
    public void Tick()
    {
        Vector2 position = inputService.MousePosition;
        position += TileDetectionOffset;
        
        position /= Global.PixelsPerUnit;

        Vec2I pointedCell = new((int)Math.Floor(position.X), (int)Math.Floor(position.Y));

        if (!map.Contains(pointedCell)) return;

        world.Pool<Sprite>().Get(pointedCellOverlayEntity).Hidden = pointedCell == currentSelectedCell;

        if (inputService.IsCellSelectionPressed())
        {
            world.Pool<Sprite>().Get(selectedCellOverlayEntity).Hidden = false;
            world.Pool<Position>().Get(selectedCellOverlayEntity).Set(pointedCell.X, pointedCell.Y);
            currentSelectedCell = pointedCell;
            
            if (currentPointedCell == currentSelectedCell && TrySelectNext()) return; 
            
            selectionQueue.Clear();
            foreach (int entity in map.GetEntitiesAt(pointedCell).Reverse()) 
                selectionQueue.Enqueue(entity);
        }
        
        if (currentPointedCell == pointedCell)
            return;

        world.Pool<Position>().Get(pointedCellOverlayEntity).Set(pointedCell.X, pointedCell.Y);
    }

    private bool TrySelectNext()
    {
        if (selectionQueue.TryDequeue(out var entity))
        {
            if (currentSelectedEntity != -1)
            {
                world.Pool<Selected>().Remove(currentSelectedEntity);
            }

            world.Pool<Selected>().New(entity);
            currentSelectedEntity = entity;
            return true;
        }

        return false;
    }
}