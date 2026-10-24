using destruct.DI;
using destruct.imgui;

namespace mix.code_base;

public sealed class AnimatorEditor : Scene, IDependenciesProvider
{
    public override IDependenciesProvider[] GetProviders() => [this]; 

    protected override void OnSceneLoaded()
    {
        
    }

    public override void Tick()
    {
        deGUI.BeginGUI();
        {
        }
        deGUI.EndGUI();
    }
}



