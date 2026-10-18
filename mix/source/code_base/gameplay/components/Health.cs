namespace mix.code_base;

public struct Health
{
    public int Current;
    public int Maximum;

    public void Set(int current, int maximum)
    {
        Current = current;
        Maximum = maximum;
    }
}