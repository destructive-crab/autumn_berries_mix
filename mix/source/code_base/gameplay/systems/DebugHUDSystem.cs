using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.imgui;
using destruct.scenes;
using Raylib_cs;

namespace mix.code_base.gameplay.systems;

public class DebugHUDSystem : IPostWorldRendererSystem
{
    public string DebugName() => nameof(DebugHUDSystem);
    
    private IFilter selectedCellFilter;
    private CellSelectionSystemsRoot selectionRoot;

    private ECSWorld world;

    [Inject]
    private void Inject(ECSWorld world, CellSelectionSystemsRoot selectionRoot)
    {
        this.selectionRoot = selectionRoot; 
        this.world = world;
        
        selectedCellFilter = world.Filter()
            .In<Cell>()
            .In<Selected>()
            .Evaluate();
    }

    private string inputText = "test";
    private InputFieldRuntimeData inputFieldData;
    
    private void Draw()
    {
        //debug info
        deGUI.BeginGUI();
        {
            if (selectedCellFilter.Entities.Count() != 0)
            {
                deGUI.BeginContainer(deGUI.ConW() - 300, 0, 300, 120);
                {
                    deGUI.FillContainer(Color.Black);
                    ref Cell cell = ref world.GetFrom<Cell>(selectedCellFilter.Entities.Last());
                    deGUI.TextXCenter(selectionRoot?.Current?.DebugName(), 20, 20, Color.White);
                    deGUI.YSpace(20);
                    deGUI.TextXCenter(cell.Position.ToString(), deGUI.NextY(), 20, Color.White);
                    deGUI.YSpace(10);
                    deGUI.TextXCenter(UTL.ToStringEnumerable(cell.Entities), deGUI.NextY(), 20, Color.White);
                }
                deGUI.EndContainer();
            }

            deGUI.BeginContainer(40, deGUI.CenteredYFor(30), deGUI.ConW() - 80, 30);
            {
                if (deGUI.InputField(inputText, 0, 0, deGUI.ConW(), ref inputFieldData, out inputText))
                {
                    LGR.PM(inputText);
                }
            }
            deGUI.EndContainer();
        }
        deGUI.EndGUI();
        
    }

    public void PostWorldRenderer()
    {
        Draw();
    }
}