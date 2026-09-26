using System.Numerics;

namespace destruct;

public class LevelInputService : ILevelInputService
{
    public event Action OnTileSelected;
    public event Action OnRightMouseButtonUp;
    public Vector2 MousePosition { get; private set; }
    public Vector2 MouseDelta { get; private set; }
    public Vector2 CameraMovement { get; private set; }
    public int CameraZoom { get; private set; }
}