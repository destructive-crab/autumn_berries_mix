using Godot;

namespace destructive_code.Tilemap;

public partial class OverlayNode : Node2D
{
    public virtual void OnCellPointed() {}
    public virtual void OnCellNotPointed() {}
    public virtual void OnCellSelected() {}
    public virtual void OnCellDeselected() {}
}