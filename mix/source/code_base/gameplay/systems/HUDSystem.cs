using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.imgui;
using destruct.scenes;
using Raylib_cs;

namespace mix.code_base.gameplay.systems;

public sealed class HUDSystem : ISceneSystem, ISceneSystemPostWorldRenderer 
{
    public string DebugName() => nameof(HUDSystem);

    private ECSWorld world;

    private string PanelDescriptionText;
    private string PanelLabelText;
    
    [Inject]
    public void Inject(ECSWorld world)
    {
        this.world = world;
        world.Filter()
            .In<Tile>()
            .In<Selected>()
            .Subscribe(SelectedTilesChanged)
            .Evaluate();
    }

    private void SelectedTilesChanged(IEnumerable<int> entities)
    {
        PanelLabelText = "";
        PanelDescriptionText = "";
        
        if (entities.Count() > 1)
        {
            PanelLabelText = "Multiple selection";
            PanelDescriptionText = $"{entities.Count()} tiles selected";
        }
        else if (entities.Count() == 1)
        {
            int entity = entities.First();
            if (world.Pool<Name>().Has(entity))
            {
                PanelLabelText = world.Pool<Name>().Get(entity).Value;
            }
            if (world.Pool<Description>().Has(entity))
            {
                PanelDescriptionText = deGUI.WrapText(world.Pool<Description>().Get(entity).Value, 350, 20);
            }
        }
    }

    public void PostWorldRenderer()
    {
        deGUI.BeginGUI();
        {
            deGUI.BeginContainer(0, deGUI.ConH()-140, 350, 140);
            {
                deGUI.ContainerBorder();
                deGUI.FillContainer(new Color(0, 0, 0, 100));
                deGUI.TextCenter("Actions panel", 20, Color.White);
            }
            deGUI.EndContainer();
            
            deGUI.BeginContainer(370, deGUI.ConH()-100, 400, 100);
            {
                deGUI.ContainerBorder();
                deGUI.FillContainer(new Color(0, 0, 0, 100));
                deGUI.TextXCenter(PanelLabelText, 10, 30, Color.White);
                
                deGUI.TextCenter(PanelDescriptionText, 20, Color.White);
            }
            deGUI.EndContainer();
        }
        deGUI.EndGUI();
    }
}