using System.Numerics;
using destruct.inputs;

namespace mix.inputs;

public interface ILevelInputService : IInputService
{
    public event Action OnTileSelected; 
    
    public Vector2 CameraMovement { get; }
    public int CameraZoom { get; }
}
