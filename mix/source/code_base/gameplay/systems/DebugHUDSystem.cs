using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.imgui;
using destruct.scenes;
using Raylib_cs;

namespace mix.code_base.gameplay.systems;

public sealed class DebugHUDSystem : IGUISystem
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
    
    public void DrawGUI()
    {
        deGUI.Text((1f / Game.DeltaTime).ToString() + "fps", 20, 10, 10, Color.White);
        return;
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
    }
}