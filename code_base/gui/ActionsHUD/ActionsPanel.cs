using System.Collections.Generic;
using autumn_berries_mix.Level;
using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using Godot;

namespace destructive_code.UI;

public sealed partial class ActionsPanel : GridContainer
{
    [Export] private PackedScene actionButton;
    [Export] private int buttonsCount = 8;

    public TileAction SelectedAction { get; private set; }
    
    private readonly List<TileActionButton> buttons = new();
    
    private INodeFactory nodeFactory;
    private ECSWorld world;

    [Inject]
    private void Ready(INodeFactory nodeFactory, ECSWorld world)
    {
        this.nodeFactory = nodeFactory;
        this.world = world;
        
        PopulateButtons();

        world.Pool<Selected>().OnNew += OnNew;
    }

    private void OnNew(EntityComponentPool<Selected> pool, int entityId)
    {
        if (world.Pool<Selectable>().Has(entityId))
        {
            OnTileSelected(world.Pool<Selectable>().Get(entityId).TileActions());
        }
    }

    private void OnTileSelected(TileAction[] actions)
    {
        SelectedAction?.Deselect();
        SelectedAction = null;
        
        Load(actions);
    }

    public override void _ExitTree()
    {
        world.Pool<Selected>().OnNew -= OnNew;
    }

    public void SelectAction(TileAction action)
    {
        SelectedAction?.Deselect();
        
        if (SelectedAction == action)
        {
            SelectedAction = null;
            return;
        }

        SelectedAction = action;
        SelectedAction.Select();
    }
    
    public void Load(TileAction[] actions)
    {
        if (buttons.Count < actions.Length)
        {
            LGR.PERR($"Actions count must be under {buttonsCount}. Trying to show {actions.Length} actions");
        }

        for (var i = 0; i < buttons.Count; i++)
        {
            if (i < actions.Length)
            {
                buttons[i].Bind(this, actions[i]);
                buttons[i].Show();
                
                continue;
            }

            buttons[i].Hide();
        }
    }

    private void PopulateButtons()
    {
        for (int i = 0; i < buttonsCount; i++)
        {
            TileActionButton b = nodeFactory.Instantiate<TileActionButton>(actionButton, this);
            b.Hide();
            
            buttons.Add(b);
        }
    }
}