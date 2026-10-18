using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;
using mix.code_base.gameplay.systems;

namespace mix.gameplay.overlay.units.abilities;

public abstract class PlayerAbilitySystem<TAbility> : ISystem, ISystemBasedOnCellSelection, IPlayerTurnSystem
    where TAbility : struct 
{
    public abstract string DebugName();
    
    protected ECSWorld World { get; private set; }
    protected GameMap  Map   { get; private set; }

    protected IEnumerable<int> CurrentSelected => currentSelected;
    
    private List<int> currentSelected = new();
    private IFilter filter;

    protected PlayerTurn CurrentTurn => TurnState.CurrentTurn as PlayerTurn;
    protected TurnState TurnState { get; private set; }
    protected CellSelectionSystemsRoot SelectionRoot { get; private set; }

    [Inject]
    protected void Inject(ECSWorld world, GameMap map, TurnState turnState, CellSelectionSystemsRoot selectionRoot)
    {
        World = world;
        Map = map;
        TurnState = turnState;
        SelectionRoot = selectionRoot;
        
        filter = world.Filter()
            .In<PlayerUnitAbility>()
            .In<Selected>()
            .In<TAbility>()
            .Subscribe(UpdateSelectedAbilities)
            .Evaluate();
    }

    public void Active() { }

    public void OnCellSelected(int cell)
    {
        Vec2I cellPosition = World.GetFrom<Position>(cell).ToRoundedV2I();

        if (!Map.CellHasEntityWith<BlockCell>(cellPosition))
        {
            OnFreeCellSelected(cellPosition);
        }
        else
        {
            OnBlockedCellSelected(cellPosition);
        }
        
        if (Map.CellHasEntityWith<Health>(cellPosition))
        {
            OnCellWithHealthSelected(cellPosition);
        }
    }

    protected int UnitOf(int ability) 
        => World.GetFrom<PlayerUnitAbility>(ability).UnitId;

    protected Vec2I PositionOf(int ability)
        => World.GetFrom<Cell>(World.GetFrom<Tile>(UnitOf(ability)).CellId).Position;

    protected ref TAbility AbilityComponent(int ability)
        => ref World.GetFrom<TAbility>(ability);

    private void UpdateSelectedAbilities(IEnumerable<int> abilities)
    {
        List<int> newlySelected = new();

        foreach (int ability in abilities)
        {
            if (!CurrentSelected.Contains(ability))
                newlySelected.Add(ability);
        }

        if (currentSelected.Count() != 0)
        {
            DeselectAbilities(currentSelected);
        }

        if (newlySelected.Count() != 0)
        {
            SelectAbilities(newlySelected);
        }

        if (abilities.Count() == 0 && SelectionRoot.Current == this)
        {
            SelectionRoot.FocusPrevious();
        }
        else if (abilities.Count() != 0)
        {
            SelectionRoot.FocusOn(this);
        }
        
        currentSelected = new(abilities);
    }

    protected abstract void SelectAbilities(IEnumerable<int> selectedAbilities);
    protected abstract void DeselectAbilities(IEnumerable<int> deselectedAbilities);
    
    protected virtual void OnFreeCellSelected(Vec2I cellPosition) {}
    protected virtual void OnBlockedCellSelected(Vec2I cellPosition) {}
    protected virtual void OnCellWithEnemySelected(Vec2I cellPosition) {}
    protected virtual void OnCellWithHealthSelected(Vec2I cellPosition) {}

    protected void DeselectCurrent()
    {
        foreach (int ability in CurrentSelected)
        {
            World.RemoveFrom<Selected>(ability);
        }
    }

    public void Begin(PlayerTurn turn)
    {
        
    }

    public void End(PlayerTurn turn)
    {
        
    }
}