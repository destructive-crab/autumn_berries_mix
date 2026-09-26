using System.Numerics;
using destruct.DI;
using mix.code_base.gameplay;
using Raylib_cs;

namespace mix.inputs;

public sealed class UltimateInputService : IGeneralInputService, IGridInputService, IGameplayInputService, ICameraInputSystem
{
    [Inject] private Camera camera;

    public Vector2 MousePosition => Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera.CurrentCamera);
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