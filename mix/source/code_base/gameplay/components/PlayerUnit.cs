
namespace destruct;

public struct PlayerUnit
{
    public int[] Abilities;
    public int PreviousSelectedAbility;

    public PlayerUnit()
    {
        Default();
    }

    public void Default()
    {
        PreviousSelectedAbility = -1;
    }
}
