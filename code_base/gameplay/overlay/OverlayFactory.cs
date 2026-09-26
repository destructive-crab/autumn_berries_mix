using System;
using destruct;
using destruct.Debug;
using Godot;

namespace destructive_code.Tilemap;

public sealed class OverlayFactory
{
    private PackedScene arrowScene;
    
    private PackedScene selectedCellPackedScene;
    private PackedScene pointedCellPackedScene;

    private OverlayNode pointedCellOverlayInstance;
    private OverlayNode selectedCellOverlayInstance;
    
    private readonly LevelScene level;

    public OverlayFactory(LevelScene level)
    {
        this.level = level;
    }

    public void LoadResources()
    {
        arrowScene = GD.Load<PackedScene>("res://overlay/arrow.tscn");
        
        selectedCellPackedScene = GD.Load<PackedScene>("res://overlay/selected_cell.tscn");
        pointedCellPackedScene  = GD.Load<PackedScene>("res://overlay/pointed_cell.tscn");
    }

    public OverlayNode CreateArrow(Vector2 position, Vector2 direction)
    {
        OverlayNode i = arrowScene.Instantiate<OverlayNode>();
        level.AddChild(i);
        
        i.Position = position * Global.TILE_SIZE + new Vector2I(Global.TILE_SIZE/2, Global.TILE_SIZE/2);
        i.Rotate(direction.Angle() - Single.Pi);

        return i;
    }

    public void SetSelection(Vector2 position)
    {
        if (selectedCellOverlayInstance == null)
        {
            selectedCellOverlayInstance = selectedCellPackedScene.Instantiate<OverlayNode>();
            level.AddChild(selectedCellOverlayInstance);
        }

        selectedCellOverlayInstance.Position =
            position * Global.TILE_SIZE + new Vector2I(Global.TILE_SIZE / 2, Global.TILE_SIZE / 2);
    }

    public void SetPointing(Vector2 position)
    {
        if (pointedCellOverlayInstance == null)
        {
            pointedCellOverlayInstance = pointedCellPackedScene.Instantiate<OverlayNode>();
            level.AddChild(pointedCellOverlayInstance);
        }

        pointedCellOverlayInstance.Position =
            position * Global.TILE_SIZE + new Vector2I(Global.TILE_SIZE / 2, Global.TILE_SIZE / 2);
    }
}