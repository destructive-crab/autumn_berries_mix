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
        deGUI.NewStyle()
            .ImportBorder(8, "assets/gui/styles/swag/border.png")
            .SetDefaultButtonIdleColor(Color.Gray)
            .SetDefaultButtonHoveredColor(Color.White)
            .SetDefaultButtonPressedColor(Color.Gray);
    }

    public override async void Tick()
    {
        deGUI.BeginGUI();
        {
            deGUI.FillContainer(Color.Magenta);
            deGUI.BeginContainerCenter(deGUI.ConW()/3 * 2, deGUI.ConH()/3 * 2);
            {
                deGUI.FillContainer(Color.Purple);
                
                int buttonH = 30;

                deGUI.BeginMarginContainerX(30);
                {
                    deGUI.TextXCenter("AUTUMN BERRIES MIX", deGUI.RelativeCenterY()-80, 30, Color.Black);
                    
                    var startPressed = deGUI.ButtonWFull("Start", deGUI.CenteredYFor(buttonH), buttonH);
                    
                    if (startPressed)
                    {
                        deGUI.EndGUI();
                        await Game.G<SceneSwitcher>().SwitchTo(new TestScene());
                        return;
                    }
                    
                    deGUI.ElementBorder();
                    
                    deGUI.YSpace(20);
                    
                    deGUI.ButtonWFull("Editor", deGUI.NextY(), buttonH);
                    deGUI.ElementBorder();
                    
                    deGUI.YSpace(20);
                    
                    deGUI.ButtonWFull("Exit", deGUI.NextY(), buttonH);
                    deGUI.ElementBorder();
                }
                deGUI.EndContainer();
                
                deGUI.ContainerBorder();
            }
            deGUI.EndContainer();
        }
        deGUI.EndGUI();
    }
}