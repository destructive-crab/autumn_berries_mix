using destruct;
using destruct.DI;
using mix.code_base;
using mix.code_base.gameplay.systems;
using mix.gameplay.systems;

namespace mix.gameplay.overlay.units.abilities;

public sealed class BasicAttackSystem : PlayerAbilitySystem<BasicAttackAbility>
{
    public override string DebugName() => nameof(BasicAttackSystem);

    [Inject] private OverlayFactory overlayFactory;
    private AreaTracker areaTracker;

    public BasicAttackSystem()
    {
        areaTracker = Game.G<DIKernel>().InjectWithBase(new AreaTracker());
    }

    protected override void SelectAbilities(IEnumerable<int> selectedAbilities)
    {
        foreach (int ability in selectedAbilities)
        {
            var area = areaTracker.TrackWith<Health>(ability, PositionOf(ability), c => true);
            areaTracker.RegisterDependenciesThatWillBeDeletedWhenTrackStoppedFor(ability, overlayFactory.SpawnAttackPointers(area));
        }
    }

    protected override void DeselectAbilities(IEnumerable<int> deselectedAbilities)
    {
        foreach (int ability in deselectedAbilities)
            areaTracker.StopTrack(ability);
    }

    protected override void OnCellWithHealthSelected(Vec2I cellPosition)
    {
        if (areaTracker.TryGetAreaIdIfCellInIt(cellPosition, out var ability))
        {
            PerformHit(ability, cellPosition);
        }
    }

    private void PerformHit(int ability, Vec2I cellPosition)
    {
        int attackerUnit = UnitOf(ability);
        
        foreach (int entity in Map.GetEntities(cellPosition))
        {
            if (World.HasIn<Health>(entity))
            {
                World.GetFrom<Health>(entity).Current -= AbilityComponent(ability).Damage;
            }
        }

        //if (World.HasIn<Animator>(attackerUnit))
        //{
        //    ref Animator animator = ref World.GetFrom<Animator>(attackerUnit);
        //    
        //    if (AnimatorUTL.AnimatorHasState(World, attackerUnit, "basic_attack"))
        //        animator.HasTrigger("basic_attack");
        //}
        
        World.RemoveFrom<Selected>(ability);
        CurrentTurn.RemovePoints(1);
    }
}