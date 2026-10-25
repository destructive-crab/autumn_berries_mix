using destruct.DI;
using destruct.scenes;
using mix.gameplay.overlay;
using mix.inputs;
using Raylib_cs;

namespace mix.gameplay.systems;

public sealed class CameraControlsSystem : ISystem, IInputsProcessSystem 
{
    public string DebugName() => nameof(CameraControlsSystem);

    [Inject] private Camera camera;
    [Inject] private ICameraInputSystem inputService;

    public void ProcessInputs(bool guiWasOverlapped)
    {
        if (guiWasOverlapped) return;
        
        camera.CurrentCamera.Target -= inputService.GetCameraPositionDelta()/camera.CurrentCamera.Zoom;
        camera.CurrentCamera.Zoom += inputService.GetCameraZoomDelta()/100;
        camera.CurrentCamera.Offset = new(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
    }
}