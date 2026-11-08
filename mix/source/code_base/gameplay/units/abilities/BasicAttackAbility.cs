namespace mix.gameplay.overlay.units.abilities;

public struct BasicAttackAbility
{
    public int Damage;
    public string SoundPath;

    public BasicAttackAbility(int damage, string assetPath)
    {
        Damage = damage;
        SoundPath = assetPath;
    }
}