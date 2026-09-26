using destruct.DI;
using destruct.scenes;
using mix.inputs;
using Raylib_cs;

namespace mix.code_base.gameplay.systems;

public sealed class CameraControlsSystem : ISceneSystem, ISceneSystemTick
{
    public string DebugName() => nameof(CameraControlsSystem);

    [Inject] private Camera camera;
    [Inject] private ICameraInputSystem inputService;

    public void Tick()
    {
        camera.CurrentCamera.Target -= inputService.GetCameraPositionDelta()/(camera.CurrentCamera.Zoom)*2;
        camera.CurrentCamera.Zoom += inputService.GetCameraZoomDelta()/10;
        camera.CurrentCamera.Offset = new(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
    }
}