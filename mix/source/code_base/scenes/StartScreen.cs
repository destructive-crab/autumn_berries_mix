using destruct;
using destruct.DI;
using destruct.imgui;
using Raylib_cs;

namespace mix.code_base;

public sealed class StartScreen : Scene, IDependenciesProvider
{
    public override IDependenciesProvider[] GetProviders() 
        => [this];

    protected override void OnSceneLoaded()
    {
        deGUI.Font = Raylib.LoadFont("assets/font.ttf");
    }

    public override void Tick()
    {
        deGUI.BeginGUI();
        {
            if (deGUI.Button("Test Scene", deGUI.CenterX()-100, deGUI.CenterY() - 15, 200, 30))
            {
                Game.G<SceneSwitcher>().SwitchTo(new TestScene());
            }
        }
        deGUI.EndGUI();
    }
}