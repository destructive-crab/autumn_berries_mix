using destruct;
using destruct.DI;
using Godot;

namespace destruct;

public sealed class NodeFactory : INodeFactory
{
    public T Instantiate<T>(PackedScene scene, Node parent, bool inject = true) 
        where T : Node
    {
        T i = scene.Instantiate<T>();
        parent.AddChild(i);
        
        if(inject)
        {
            InjectNodeTree(i);
        }

        return i;
    }

    public TNode InjectNodeTree<TNode>(TNode node) 
        where TNode : Node
    {
        GD.Print($"inject in {node}");
        Game.G<DIKernel>().InjectWithBase(node);
        GD.Print($"injecting children");
        foreach (Node child in node.GetChildren())
        {
            GD.Print($"inject child {child.Name}");
            InjectNodeTree(child);
        }

        return node;
    }
}