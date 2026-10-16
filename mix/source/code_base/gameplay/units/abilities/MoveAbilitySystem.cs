using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.code_base;
using mix.code_base.gameplay.systems;
using mix.gameplay.overlay.tweens;
using mix.gameplay.systems;

namespace mix.gameplay.overlay.units.abilities;

public abstract class PlayerAbilitySystem<TAbility> : ISceneSystem, ISystemBasedOnCellSelection, IPlayerTurnSystem
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

    [Inject]
    protected void Inject(ECSWorld world, GameMap map, TurnState turnState)
    {
        World = world;
        Map = map;
        TurnState = turnState;
        
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
    }

    protected int UnitOf(int ability) 
        => World.GetFrom<PlayerUnitAbility>(ability).UnitId;

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

        currentSelected = new(abilities);
    }

    protected abstract void SelectAbilities(IEnumerable<int> selectedAbilities);
    protected abstract void DeselectAbilities(IEnumerable<int> deselectedAbilities);
    
    protected virtual void OnFreeCellSelected(Vec2I cellPosition) {}
    protected virtual void OnBlockedCellSelected(Vec2I cellPosition) {}
    protected virtual void OnCellWithEnemySelected(Vec2I cellPosition) {}

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

public sealed class MoveAbilitySystem : PlayerAbilitySystem<MoveAbility>
{
    public override string DebugName() => nameof(MoveAbilitySystem);

    private readonly Dictionary<int, List<int>> overlay = new();
    private readonly Dictionary<int, IEnumerable<Vec2I>> availableCells = new();

    [Inject] private readonly GameMap map;
    [Inject] private readonly OverlayFactory overlayFactory;
    
    private CellSelectionSystemsRoot selectionSystem;

    [Inject]
    private void Inject(CellSelectionSystemsRoot selectionSystem)
    {
        this.selectionSystem = selectionSystem;
    }
    
    protected override void SelectAbilities(IEnumerable<int> selectedAbilities)
    {
        foreach (int ability in selectedAbilities)
        {
            Vec2I position = World.GetFrom<Position>(UnitOf(ability)).ToRoundedV2I(); 
            IEnumerable<Vec2I> availableCells = map
                .GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<BlockCell>(position);
            
            overlay.Add(ability, overlayFactory.SpawnArrows(availableCells, position));
            this.availableCells.Add(ability, availableCells);
        }

        if (selectionSystem.Current != this)
        {
            selectionSystem.FocusOn(this);
        }
    }

    protected override void DeselectAbilities(IEnumerable<int> deselectedAbilities)
    {
        foreach (int ability in deselectedAbilities)
        {
            foreach (int id in overlay[ability]) World.Delete(id);
            
            overlay.Remove(ability);
            availableCells.Remove(ability);
        }

        if (CurrentSelected.Count() == 0 && deselectedAbilities.Count() != 0)
        {
            selectionSystem.FocusPrevious();
        }
    }

    protected override void OnFreeCellSelected(Vec2I cellPosition)
    {
        bool moveWasPerformed = false;
        
        foreach (var pair in availableCells)
        {
            if (pair.Value.Contains(cellPosition))
            {
                PerformMove(pair.Key, cellPosition);
                moveWasPerformed = true;
                break;
            }
        }

        if (!moveWasPerformed)
        {
            DeselectCurrent();
            selectionSystem.FocusPrevious();
        }
    }

    protected override void OnBlockedCellSelected(Vec2I cellPosition)
    {
        DeselectCurrent();
        selectionSystem.FocusPrevious();
    }

    private void PerformMove(int abilityId, Vec2I destPosition)
    {
        int unit = UnitOf(abilityId);
        World.Pool<PositionTween>().New(unit, (ref PositionTween t) =>
        {
            t.Destination = destPosition;
            t.Speed = 10;
            
            if (World.HasIn<Animator>(unit) && World.GetFrom<Animator>(unit).AnimationsContainer.Contains("move"))
            {
                World.GetFrom<Animator>(unit).Play("move");
                t.Completed += () => World.GetFrom<Animator>(unit).Play("idle"); 
            }
        });

        map.MoveTile(UnitOf(abilityId), destPosition);
        World.Pool<Selected>().Remove(abilityId);
        selectionSystem.FocusPrevious();
        CurrentTurn.RemovePoints(1);
    }
}