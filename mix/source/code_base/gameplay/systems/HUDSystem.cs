using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.imgui;
using destruct.scenes;
using mix.code_base;
using mix.code_base.gameplay.systems;
using mix.gameplay.overlay.units.abilities;
using Raylib_cs;

namespace mix.gameplay.systems;

public sealed class HUDSystem : ISceneSystem, ISceneSystemPostWorldRenderer 
{
    public string DebugName() => nameof(HUDSystem);

    private ECSWorld world;
    private CellSelectionSystemsRoot root;

    private string PanelDescriptionText;
    private string PanelLabelText;

    private int[] playerUnitAbilities;
    private int selectedAbility = -1;
    
    private IFilter selectedCellFilter;
    private TurnState turnState;

    [Inject]
    public void Inject(ECSWorld world, CellSelectionSystemsRoot root, TurnState turnState)
    {
        this.world = world;
        this.root  = root;
        this.turnState = turnState;
        
        world.Filter()
            .In<Tile>()
            .In<Selected>()
            .Subscribe(SelectedTilesChanged)
            .Evaluate();
        
        selectedCellFilter = world.Filter()
            .In<Cell>()
            .In<Selected>()
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

            if (world.Pool<PlayerUnit>().Has(entity))
            {
                ref PlayerUnit unit = ref world.GetFrom<PlayerUnit>(entity);
                playerUnitAbilities = unit.Abilities;
                selectedAbility = unit.PreviousSelectedAbility;
            }
            else
            {
                playerUnitAbilities = [];
                selectedAbility = -1;
            }
        }
    }

    public void PostWorldRenderer()
    {
        deGUI.BeginGUI();
        {
            if (turnState.CurrentTurn is PlayerTurn playerTurn)
            {
                deGUI.TextXCenter($"Points left {playerTurn.PointsLeft}", 30, 30, Color.White);
                deGUI.Text((1f / Game.DeltaTime).ToString() + "fps", 20, 10, 10, Color.White);
                deGUI.BeginContainer(0, deGUI.ConH() - 140, 350, 140);
                {
                    deGUI.ContainerBorder();
                    deGUI.FillContainer(new Color(0, 0, 0, 100));
                    if (playerUnitAbilities != null)
                    {
                        selectedAbility = -1;
                        foreach (var ability in playerUnitAbilities)
                        {
                            PlayerUnitAbility abilityComponent = world.GetFrom<PlayerUnitAbility>(ability);
                            if (!world.Pool<Selected>().Has(ability))
                            {
                                if (deGUI.ImageButton(abilityComponent.Icon, 10 + deGUI.NextX(), 10, 60, 60))
                                {
                                    if (selectedAbility != -1)
                                    {
                                        world.Pool<Selected>().Remove(selectedAbility);
                                    }

                                    selectedAbility = ability;
                                    world.Pool<Selected>().New(ability);
                                }
                            }
                            else
                            {
                                selectedAbility = ability;
                                if (deGUI.ImageButton(abilityComponent.SelectedIcon, 10 + deGUI.NextX(), 10, 60, 60))
                                {
                                    world.Pool<Selected>().Remove(selectedAbility);
                                    selectedAbility = -1;
                                }
                            }
                        }
                    }
                }
                deGUI.EndContainer();
                deGUI.BeginContainer(370, deGUI.ConH() - 100, 400, 100);
                {
                    deGUI.ContainerBorder();
                    deGUI.FillContainer(new Color(0, 0, 0, 100));
                    deGUI.TextXCenter(PanelLabelText, 10, 30, Color.White);
                    deGUI.TextCenter(PanelDescriptionText, 20, Color.White);
                }
                deGUI.EndContainer();
                if (selectedCellFilter.Entities.Count() != 0)
                {
                    deGUI.BeginContainer(deGUI.ConW() - 300, 0, 300, 120);
                    {
                        deGUI.FillContainer(Color.Black);
                        ref Cell cell = ref world.GetFrom<Cell>(selectedCellFilter.Entities.Last());
                        deGUI.TextXCenter(root?.Current?.DebugName(), 20, 20, Color.White);
                        deGUI.YSpace(20);
                        deGUI.TextXCenter(cell.Position.ToString(), deGUI.NextY(), 20, Color.White);
                        deGUI.YSpace(10);
                        deGUI.TextXCenter(UTL.ToStringEnumerable(cell.Entities), deGUI.NextY(), 20, Color.White);
                    }
                    deGUI.EndContainer();
                }
                
            }
            else if (turnState.CurrentTurn is EnemyTurn enemyTurn)
            {
                deGUI.BeginContainerCenter(160, 60);
                {
                    deGUI.FillContainer(Color.Pink);
                    deGUI.ContainerBorder();
                    deGUI.TextCenter("WAIT " + enemyTurn.TimeLeft.ToString(), enemyTurnFontSize, Color.DarkBlue);
                }
                deGUI.EndContainer();
            }
        }

        deGUI.EndGUI();
    }

    private int enemyTurnFontSize = 40;
    private bool decrease = false;
}