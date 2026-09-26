using destruct.DI;
using Godot;

namespace destruct.Level;

public sealed partial class Camera : Camera2D
{
    [Inject] private ILevelInputService levelInputService;
    
    public override void _Process(double delta)
    {
        if (levelInputService != null)
        {
            Position += levelInputService.CameraMovement/Zoom.X*2f;
        
            float zoom = Mathf.Clamp(Zoom.X + levelInputService.CameraZoom/4f, 1, 8);
            Zoom = new Vector2(zoom, zoom);
        }
    }
}