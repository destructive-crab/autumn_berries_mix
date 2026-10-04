using Raylib_cs;

namespace mix.code_base;

public struct PlayerUnitAbilities
{
    public PlayerUnitAbility[] Abilities;
}

public abstract class PlayerUnitAbility
{
    public abstract Texture2D Icon();
    
    public abstract bool Available();
    
    public abstract void Selected();
    public abstract void Deselected();
}