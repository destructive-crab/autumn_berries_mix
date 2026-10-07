using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using mix.code_base;
using Raylib_cs;

namespace mix.gameplay.overlay.units;

public struct BlockCell { }

public sealed class MoveAbility : PlayerUnitAbility
{
    public readonly int AttachedTo;
    
    private TextureFragment icon;
    private TextureFragment selectedIcon;

    [Inject] private OverlayFactory overlayFactory;
    [Inject] private ECSWorld world;
    [Inject] private GameMap gameMap;

    private List<int> overlayEntities = new();
    private IEnumerable<Vec2I> availableCells;

    public MoveAbility(int attachedTo)
    {
        AttachedTo = attachedTo;
        
        icon = TextureFragment.FromZeroTo(Assets.LoadTexture("assets/units/player/move_ability.png"), Global.GUIAbilityIcon, Global.GUIAbilityIcon);
        selectedIcon = TextureFragment.WithRect(Assets.LoadTexture("assets/units/player/move_ability.png"), Global.GUIAbilityIcon, 0, Global.GUIAbilityIcon, Global.GUIAbilityIcon);
    }

    public override TextureFragment Icon() => icon;
    public override TextureFragment SelectedIcon() => selectedIcon;

    public override bool Available() => true;

    public override void Select()
    {
        int x = (int)world.Pool<Position>().Get(AttachedTo).X;
        int y = (int)world.Pool<Position>().Get(AttachedTo).Y;

        var availableToMove = gameMap.GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<BlockCell>(x,y);
        
        foreach (Vec2I cellPosition in availableToMove)
        {
            overlayEntities.Add(overlayFactory.SpawnArrow(cellPosition.X, cellPosition.Y, cellPosition - new Vec2I(x, y)));
        }
    }

    public override void Deselect()
    {
        foreach (int entity in overlayEntities)
        {
            world.Delete(entity);
        }
    }
}