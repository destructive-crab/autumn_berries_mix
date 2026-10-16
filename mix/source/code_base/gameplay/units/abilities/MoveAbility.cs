using destruct.Debug;

namespace mix.gameplay.overlay.units.abilities;

public struct PlayerUnitAbility
{
    public int UnitId;
    public int Cost;
    
    public TextureFragment Icon;
    public TextureFragment SelectedIcon;

    public PlayerUnitAbility(int unitId, string pathToIconsSheet)
    {
        UnitId = unitId;
        
        Icon = TextureFragment.FromZeroTo(Assets.LoadTexture(pathToIconsSheet), 
            Global.IconStandardSize, Global.IconStandardSize);
        SelectedIcon = TextureFragment.WithRect(Assets.LoadTexture(pathToIconsSheet), 
            Global.IconStandardSize, 0, Global.IconStandardSize, Global.IconStandardSize); 
    }
}

public struct MoveAbility
{
    public bool AllowDiagonal;

    public MoveAbility()
    {
        AllowDiagonal = true;
    }
}