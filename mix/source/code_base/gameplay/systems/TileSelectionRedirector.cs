using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;
using mix.code_base.gameplay.systems;
using mix.inputs;

public sealed class CellSelectionRedirector : ISystemBasedOnCellSelection, ISceneSystem
{
    public string DebugName() => nameof(CellSelectionRedirector);

    [Inject] private IGridInputService inputService;
    [Inject] private GameMap           map;
    [Inject] private ECSWorld          world;
    
    //overlay
    private int currentSelectedTile = -1;
    
    private CellSelectionSystemsRoot root;

    [Inject]
    private void Inject(CellSelectionSystemsRoot root)
    {
        this.root = root;
        root.FocusOn(this);
    }

    public void Active()
    {
        if (root.CurrentSelectedCell != -1)
            OnCellSelected(root.CurrentSelectedCell);
    }

    public void OnCellSelected(int cell)
    {
        int topTile = world.Pool<Cell>().Get(cell).Entities[^1];

        if (topTile == currentSelectedTile) return;

        int prevSelectedTile = currentSelectedTile;
        currentSelectedTile = topTile;
        
        if (prevSelectedTile != -1)
            world.Pool<Selected>().Remove(prevSelectedTile);
        
        world.Pool<Selected>().New(topTile);
    }
}