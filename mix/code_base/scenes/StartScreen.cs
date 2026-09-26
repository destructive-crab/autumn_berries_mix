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
        ImGUI.Font = Raylib.LoadFont("assets/font.ttf");
    }

    public override void Tick()
    {
        ImGUI.BeginGUI();
        {
            if (ImGUI.Button("Test Scene", ImGUI.CenterX()-100, ImGUI.CenterY() - 15, 200, 30))
            {
                Game.G<SceneSwitcher>().SwitchTo(new TestScene());
            }
        }
        ImGUI.EndGUI();
    }
}