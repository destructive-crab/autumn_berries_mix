using System;
using Godot;

namespace destructive_code;

public partial class InputService : Node2D, IInputService
{
    public event Action OnTileSelected;
    public event Action OnRightMouseButtonUp;
    public Vector2 MousePosition => GetGlobalMousePosition(); 

    private const string TileSelected = "tile_selected";

    public override void _Process(double delta)
    {
        if (Input.IsActionJustReleased(TileSelected))
        {
            OnTileSelected?.Invoke();
        }
    }
}