using Godot;

namespace destructive_code;

public interface INodeFactory
{
    T Instantiate<T>(PackedScene scene, Node parent) where T : Node;
}