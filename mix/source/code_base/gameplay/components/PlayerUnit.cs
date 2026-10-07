namespace mix.code_base;

public struct PlayerUnit
{
    public PlayerUnitAbility[] Abilities;
}

public abstract class PlayerUnitAbility
{
    public abstract TextureFragment Icon();
    public abstract TextureFragment SelectedIcon();
    
    public abstract bool Available();
    
    public abstract void Select();
    public abstract void Deselect();

}