using System;
using Godot;

namespace destruct;

public partial class LevelInputService : Node2D, ILevelInputService
{
    public event Action OnTileSelected;
    public event Action OnRightMouseButtonUp;
    public Vector2 MousePosition { get; private set; }
    public Vector2 MouseDelta { get; private set; }
    public Vector2 CameraMovement { get; private set; }
    public int CameraZoom { get; private set; }

    private static readonly StringName TileSelected = "tile_selected";
    
    private static readonly StringName ScrollUp   = "scroll_up";
    private static readonly StringName ScrollDown = "scroll_down";

    private Texture2D defaultCursor;
    private Texture2D clickCursor;
    private Texture2D moveCursor;

    public override void _Ready()
    {
        defaultCursor = ResourceLoader.Load<Texture2D>("res://cursor/defaultCursor.png");
        clickCursor   = ResourceLoader.Load<Texture2D>("res://cursor/clickCursor.png");
        moveCursor    = ResourceLoader.Load<Texture2D>("res://cursor/moveCursor.png");
        
        Input.SetCustomMouseCursor(defaultCursor);
    }

    public override void _Process(double delta)
    {
        Input.SetCustomMouseCursor(defaultCursor);
        
        if (Input.IsActionJustReleased(TileSelected))
        {
            OnTileSelected?.Invoke();
        }

        CameraZoom = 0;
        if (Input.IsActionJustReleased(ScrollUp))   CameraZoom = 1;
        if (Input.IsActionJustReleased(ScrollDown)) CameraZoom = -1;

        var newMousePosition = GetGlobalMousePosition();
        CameraMovement = Vector2.Zero;

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
        }

        if (Input.IsMouseButtonPressed(MouseButton.Right))
        {
            CameraMovement = -MouseDelta/2;
            Input.SetCustomMouseCursor(moveCursor);
        }

        MousePosition = newMousePosition;
        
        MouseDelta = Vector2.Zero;
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsMouseButtonPressed(MouseButton.Right) && @event is InputEventMouseMotion mouseMotion)
        {
            MouseDelta = mouseMotion.Relative;
        }
    }
    
    
}