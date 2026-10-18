using destruct;
using destruct.DI;
using mix.code_base;
using mix.code_base.gameplay.systems;
using mix.gameplay.overlay.tweens;
using mix.gameplay.systems;

namespace mix.gameplay.overlay.units.abilities;

public sealed class MoveAbilitySystem : PlayerAbilitySystem<MoveAbility>
{
    public override string DebugName() => nameof(MoveAbilitySystem);

    private AreaTracker areaTracker;

    [Inject] private readonly OverlayFactory overlayFactory;
    
    protected override void SelectAbilities(IEnumerable<int> selectedAbilities)
    {
        if (areaTracker == null) areaTracker = new(Map, World);
        
        foreach (int ability in selectedAbilities)
        {
            Vec2I position = World.GetFrom<Position>(UnitOf(ability)).ToRoundedV2I(); 
            IEnumerable<Vec2I> cells = areaTracker.TrackWithout<BlockCell>(ability, position, c => true);
            areaTracker.RegisterDependenciesThatWillBeDeletedWhenTrackStoppedFor(ability, overlayFactory.SpawnArrows(cells, position));
        }
    }

    protected override void DeselectAbilities(IEnumerable<int> deselectedAbilities)
    {
        foreach (int ability in deselectedAbilities)
        {
            areaTracker.StopTrack(ability);
        }
    }

    protected override void OnFreeCellSelected(Vec2I cellPosition)
    {
        bool moveWasPerformed = false;
        
        areaTracker.Foreach((int abilityId, IEnumerable<Vec2I> cells) =>
        {
            if (cells.Contains(cellPosition))
            {
                PerformMove(abilityId, cellPosition);
                moveWasPerformed = true;
                return true;
            }

            return false;
        });

        if (!moveWasPerformed)
        {
            DeselectCurrent();
        }
    }

    protected override void OnBlockedCellSelected(Vec2I cellPosition)
    {
        DeselectCurrent();
    }

    private void PerformMove(int abilityId, Vec2I destPosition)
    {
        int unit = UnitOf(abilityId);

        if (World.HasIn<Sprite>(unit))
        {
            World.GetFrom<Sprite>(unit).MirroredHorizontal = World.GetFrom<Position>(unit).X > destPosition.X;
        }
        
        World.Pool<PositionTween>().New(unit, (ref PositionTween t) =>
        {
            List<AudioService.SoundLoopController> walkSounds = new(2);
            foreach (int entity in Map.GetEntities(destPosition))
            {
                if (World.HasIn<TileWalkSound>(entity))
                {
                    string soundAssetPath = World.GetFrom<TileWalkSound>(entity).SoundAssetPath;
                    AudioService.SoundLoopController loopController = new AudioService.SoundLoopController();
                    Game.AudioService.PlaySoundLoop(soundAssetPath, 0.4f, Random.Shared.Next(90, 101)/100f, loopController);
                    walkSounds.Add(loopController);
                }
            }
            t.Destination = destPosition;
            t.Speed = 2;
            
            if (World.HasIn<Animator>(unit) && World.GetFrom<Animator>(unit).AnimationsContainer.Contains("move"))
            {
                World.GetFrom<Animator>(unit).Play("move");
                t.Completed += () =>
                {
                    World.GetFrom<Animator>(unit).Play("idle");
                    foreach (var loop in walkSounds) loop.Stop();
                }; 
            }
        });

        Map.MoveTile(UnitOf(abilityId), destPosition);
        World.Pool<Selected>().Remove(abilityId);
        CurrentTurn.RemovePoints(1);
    }
}