using System.Numerics;
using destruct.DI;
using mix.gameplay.overlay;
using Raylib_cs;

namespace mix.inputs;

public sealed class UltimateInputService : IGeneralInputService, IGridInputService, IGameplayInputService, ICameraInputSystem
{
    [Inject] private Camera camera;

    public Vector2 ScreenMousePosition => Raylib.GetMousePosition();
    public Vector2 GlobalMousePosition => Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera.CurrentCamera);
    public bool LeftMouseButtonDown => Raylib.IsMouseButtonPressed(MouseButton.Left);
    public bool RightMouseButtonDown => Raylib.IsMouseButtonDown(MouseButton.Right);

    public Vector2 GetCameraPositionDelta()
    {
        if (!Raylib.IsMouseButtonDown(MouseButton.Right))
            return new(0, 0);
        
        Vector2 d = Raylib.GetMouseDelta();
        return d;
    }

    public float GetCameraZoomDelta()
    {
        return Raylib.GetMouseWheelMoveV().Y;
    }

    public bool IsCellSelectionPressed()
    {
        return Raylib.IsMouseButtonReleased(MouseButton.Left);
    }
}