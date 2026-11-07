using destruct.DI;
using destruct.ecs;
using destruct.imgui;
using destruct.scenes;
using destruct;
using destruct.gameplay.units;
using Raylib_cs;

namespace mix.gameplay.systems;

public sealed class HealthBarsSystem : IGUISystem
{
    public string DebugName() => nameof(HealthBarsSystem);

    private ECSWorld w;
    
    private IFilter playerUnitsHealthFilter;
    private IFilter enemyUnitsHealthFilter;

    private const int PaddingFromSideOfTheScreen = 20;
    
    private const int HealthBarWidth = 200;
    private const int HealthBarHeight = 60;
    
    private const int FontSize = 20;
    private const int BarHeight = 20;

    private const int TopPadding = 5;
    
    [Inject]
    private void Inject(ECSWorld w)
    {
        this.w = w;
        
        playerUnitsHealthFilter = w.Filter().In<Health>().In<PlayerUnit>();
        enemyUnitsHealthFilter  = w.Filter().In<Health>().In<EnemyUnit>();
    }
    
    public void DrawGUI()
    {
        deGUI.BeginContainer(PaddingFromSideOfTheScreen, 0, HealthBarWidth, -1);
        {
            foreach (int entity in playerUnitsHealthFilter.Entities)
            {
                DrawHealthBar(entity);
            }
        }
        deGUI.EndContainer();
        
        deGUI.BeginContainer(deGUI.ConW()-HealthBarWidth-PaddingFromSideOfTheScreen, 0, HealthBarWidth, -1);
        {
            foreach (int entity in enemyUnitsHealthFilter.Entities)
            {
                DrawHealthBar(entity);
            }
        }
        deGUI.EndContainer();
    }

    private void DrawHealthBar(int entity)
    {
        string label = $"Health {entity}";
        ref Health health = ref w.GetFrom<Health>(entity);
        
        if (w.HasIn<Name>(entity)) label = w.GetFrom<Name>(entity).Value;
        
        deGUI.BeginContainer(0, 40, HealthBarWidth, HealthBarHeight);
        deGUI.ContainerBorder();
        {
            deGUI.YSpace(TopPadding);
            int textW = Raylib.MeasureText(label, FontSize);
            deGUI.Rectangle(deGUI.CenteredXFor(textW), deGUI.NextY(), textW, FontSize, Color.Black);
            deGUI.TextXCenter(label, deGUI.NextY(), FontSize, Color.White);
            deGUI.YSpace(5);
            deGUI.BeginContainer(5, deGUI.NextY(), (int)MathF.Ceiling(health.Current / (float)health.Maximum * (HealthBarWidth-10)), BarHeight);
            {
                deGUI.FillContainer(Color.Red);
                deGUI.TextCenter($"{health.Current}/{health.Maximum}", FontSize, Color.White);
            }
            deGUI.EndContainer();
        }
        deGUI.EndContainer();
    }

}