using System.Numerics;

namespace mix.inputs;

public interface ICameraInputSystem : IGeneralInputService
{
    public Vector2 GetCameraPositionDelta();
    float GetCameraZoomDelta();
}