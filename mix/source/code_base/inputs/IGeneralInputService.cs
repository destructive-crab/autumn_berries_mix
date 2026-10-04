using System.Numerics;

namespace mix.inputs;

public interface IGeneralInputService
{
    Vector2 ScreenMousePosition { get; }
    Vector2 GlobalMousePosition { get; }
    
    bool LeftMouseButtonDown { get; }
    bool RightMouseButtonDown { get; }
}