using Godot;

namespace destructive_code;

public sealed class NodeFactory : INodeFactory
{
    public T Instantiate<T>(PackedScene scene, Node parent) where T : Node
    {
        T i = scene.Instantiate<T>();
        parent.AddChild(i);
        
        //todo DI
        
        return i;
    }
}