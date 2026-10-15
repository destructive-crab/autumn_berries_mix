using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.gameplay.systems;

namespace mix.code_base.gameplay.systems;

public interface ISystemBasedOnCellSelection : ISceneSystem
{
    void Active();
    void OnCellSelected(int cell);
}

public sealed class CellSelectionSystemsRoot : ISceneSystem
{
    public string DebugName() => nameof(CellSelectionSystemsRoot);
    
    public int CurrentSelectedCell = -1;
    
    public ISystemBasedOnCellSelection Current { get; private set; }
    private ISystemBasedOnCellSelection previous;
    
    protected ECSWorld World;
    
    private int pointedCellOverlayEntity;
    private int selectedCellOverlayEntity;

    private int selectedCell;
    
    [Inject]
    protected void Inject(ECSWorld world)
    {
        this.World = world;
        
        pointedCellOverlayEntity = world.New()
            .With(new Position(0,0,1))
            .With(new Origin(0.5f, 0.5f))
            .With(Sprite.From(AssetsPath.PointedCellBorder)).Id;
        
        selectedCellOverlayEntity = world.New()
            .With(new Position(0,0,1))
            .With(new Origin(0.5f, 0.5f))
            .With(Sprite.From(AssetsPath.SelectedCellBorder)).Id;
        
        world.Filter()
            .In<Selected>()
            .In<Cell>()
            .Subscribe(UpdateSelectedCell)
            .Evaluate();
        
        world.Filter()
            .In<Hovered>()
            .Subscribe((e) =>
        {
            if (e.Count() == 0) return;
            
            world.Pool<Sprite>().Get(pointedCellOverlayEntity).Hidden = e.Last() == CurrentSelectedCell;
            
            world.Pool<Position>().Get(pointedCellOverlayEntity).X = world.Pool<Position>().Get(e.Last()).X;
            world.Pool<Position>().Get(pointedCellOverlayEntity).Y = world.Pool<Position>().Get(e.Last()).Y;
        }).Evaluate();
    }

    private void UpdateSelectedCell(IEnumerable<int> cells)
    {
        if (cells.Count() != 1) return;

        CurrentSelectedCell = cells.Last();
        World.GetFrom<Position>(selectedCellOverlayEntity).Set(World.GetFrom<Position>(CurrentSelectedCell));
            
        Current?.OnCellSelected(CurrentSelectedCell);
        World.Pool<Sprite>().Get(pointedCellOverlayEntity).Hidden = true;
    }

    public void FocusOn(ISystemBasedOnCellSelection system)
    {
        previous = Current;
        Current = system;
        Current.Active();
    }

    public void FocusPrevious()
    {
        (Current, previous) = (previous, Current);
        Current.Active();
    }
}