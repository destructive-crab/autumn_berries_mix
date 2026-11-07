using destruct;

public partial class DevelopmentStartPoint : GameStartPoint
{
    public override bool AllowDebug() => false;

    public override async void Complete()
    {
        base.Complete();

        await Game.G<SceneSwitcher>().SwitchTo(new StartScreen());
    }
}