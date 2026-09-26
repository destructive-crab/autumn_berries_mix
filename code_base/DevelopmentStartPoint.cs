using destruct;
using Godot;

public partial class DevelopmentStartPoint : GameStartPoint
{
    public override bool AllowDebug() => false;

    public override async void Complete()
    {
        base.Complete();

        await Game.G<SceneSwitcher>().SwitchTo<LevelScene>(GD.Load<PackedScene>("res://scenes/dev.tscn"));
    }
}