
namespace mix.code_base;

public struct PlayerUnit
{
    public int[] Abilities;
    public int PreviousSelectedAbility;

    public PlayerUnit()
    {
        PreviousSelectedAbility = -1;
    }
}
