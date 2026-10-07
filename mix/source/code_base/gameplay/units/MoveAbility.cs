using destruct.ecs;
using mix.code_base;
using Raylib_cs;

namespace mix.gameplay.overlay.units;

public struct BlockCell { }

public sealed class MoveAbility : PlayerUnitAbility
{
    public readonly int AttachedTo;
    
    private CachedTexture icon;

    private OverlayFactory overlayFactory;
    private ECSWorld world;
    private GameMap gameMap;

    private List<int> overlayEntities;
    private IEnumerable<Vec2I> availableCells;

    public MoveAbility(int attachedTo)
    {
        AttachedTo = attachedTo;
        
        icon = Assets.LoadTexture("assets/units/player/move_ability.png");
    }

    public override Texture2D Icon() => icon.Get();

    public override bool Available() => true;

    public override void Selected()
    {
        int x = (int)world.Pool<Position>().Get(AttachedTo).X;
        int y = (int)world.Pool<Position>().Get(AttachedTo).Y;

        var availableToMove = gameMap.GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<BlockCell>(x,y);
        
        foreach (var cellPosition in availableToMove)
        {
            overlayFactory.SpawnArrow(x, y, cellPosition - new Vec2I(x, y));
        }
        
    }

    public override void Deselected()
    {
        
    }
}