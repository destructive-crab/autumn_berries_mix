using destructive_code.Debug;
using destructive_code.DI;
using Godot;

namespace destructive_code;

public partial class GameRoot : Node, IDependenciesProvider
{
    //for godot singleton
    public static GameRoot I { get; private set; }
    
    public DIKernel      DIKernel     { get; private set; }
    public IInputService InputService { get; private set; }
    public INodeFactory   NodeFactory   { get; private set; }

    public override void _EnterTree()
    {
        I = this;
        
        InputService = GetNode<InputService>("/root/InputService");
        NodeFactory   = new NodeFactory();

        DIKernel = new DIKernel();
        
        DIKernel.RegisterDependenciesToCore(this);
        
        LGR.PM("GAME ROOT INITIALIZED");
    }
    
    [Provide(true)]
    private IInputService ProvideInputServiceForDI()
    {
        if (InputService == null)
        {
            LGR.PERR("Invalid call order; cannot provide input service before it is initialized");
            return null;
        }
        
        return InputService;
    }
    
    [Provide(true)]
    private INodeFactory ProvideINodeFabricForDI()
    {
        if (NodeFactory == null)
        {
            LGR.PERR("Invalid call order; cannot provide node fabric before it is initialized");
            return null;
        }
        
        return NodeFactory;
    }
}