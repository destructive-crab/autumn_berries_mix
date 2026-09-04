using System;
using Godot;

public enum InputMode
{
    Main
}

public interface IInputService
{
    public event Action OnTileSelected; 
    public event Action OnRightMouseButtonUp; 
    
    public Vector2 MousePosition { get; }
}