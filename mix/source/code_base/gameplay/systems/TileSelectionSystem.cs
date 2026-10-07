using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.inputs;

namespace mix.code_base.gameplay.systems;

public sealed class TileSelectionSystem : ISceneSystem
{
    public string DebugName() => nameof(CellSelectionSystem);

    private Vec2I currentPointedCell;
    private Vec2I currentSelectedCell;
    
    private ECSWorld world;
    private GameMap map;
    
    [Inject] private IGridInputService inputService;
    
    //overlay
    private int pointedCellOverlayEntity;
    private int selectedCellOverlayEntity;

    private readonly Queue<int> selectionQueue = new();
    private int currentSelectedEntity = -1;
    
    [Inject]
    private void Inject(ECSWorld world, GameMap map)
    {
        this.world = world;
        this.map = map;
        
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
            .In<Tile>()
            .Subscribe((e) =>
        {
            world.Pool<Sprite>().Get(selectedCellOverlayEntity).Hidden = e.Count() == 0;

            if (e.Count() == 0) return;
            if (e.Count() == 1 && e.First() == currentSelectedEntity) return;
            
            int lastSelected = e.Last();

            if (currentSelectedEntity != -1)
            {
                world.Pool<Selected>().Remove(currentSelectedEntity);
            }
            
            world.Pool<Position>().Get(selectedCellOverlayEntity).X = world.Pool<Position>().Get(lastSelected).X;
            world.Pool<Position>().Get(selectedCellOverlayEntity).Y = world.Pool<Position>().Get(lastSelected).Y;
            
            currentSelectedEntity = e.Last();
        }).Evaluate();
    }
}