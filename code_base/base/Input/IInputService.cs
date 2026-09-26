using System;
using Godot;

namespace destruct;

public interface IInputService
{
    public event Action OnRightMouseButtonUp; 
    
    public Vector2 MousePosition { get; }
    public Vector2 MouseDelta { get; }
}