namespace destruct;

public struct Health
{
    public int Current;
    public int Maximum;

    public Health(int current, int maximum)
    {
        Current = current;
        Maximum = maximum;
    }

    public void Set(int current, int maximum)
    {
        Current = current;
        Maximum = maximum;
    }
}