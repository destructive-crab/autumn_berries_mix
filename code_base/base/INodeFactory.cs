using Godot;

namespace destruct;

public interface INodeFactory
{
    T Instantiate<T>(PackedScene scene, Node parent, bool inject = true) where T : Node;
    TNode InjectNodeTree<TNode>(TNode node) where TNode : Node;
}