using System.Numerics;
using destruct;

public enum InputMode
{
    Main
}

public interface ILevelInputService : IInputService
{
    public event Action OnTileSelected; 
    
    public Vector2 CameraMovement { get; }
    public int CameraZoom { get; }
}