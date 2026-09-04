using destructive_code.DI;
using destructive_code.Level;
using destructive_code.Tilemap;
using Godot;
using GodotTask;

namespace destructive_code.Debug.Units;

public sealed class ChainyTile : ITile, ISelectableTile
{
    [Inject] private NodeFactory factory;
    [Inject] private UnitsFactory unitsFactory;

    private ChainyUnit chainyUnit;
    
    public Vector2I Position { get; private set; }

    public void OnPlaced()
    {
        chainyUnit = unitsFactory.InstantiateChainy();
    }

    public void OnRemoved()
    {
        chainyUnit.Dispose();
    }

    public void UpdatePosition(Vector2I pos)
    {
        chainyUnit.Position = pos + new Vector2(Global.TILE_SIZE/2, Global.TILE_SIZE/2);
    }

    public ITile Clone()
    {
        return new ChainyTile();
    }

    public string Export()
    {
        return null;
    }

    public void Import(string json) { }

    public TileAction[] Select()
    {
        return [new TestAction()];
    }
}

//actually its not units factory, its a factory that creates nodes for tiles of units;
//todo rename to something so it will make sense 
public sealed class UnitsFactory : IGameMapService
{
    [Inject] private INodeFactory nodeFactory;
    [Inject] private LevelRoot   levelRoot;
    
    public ChainyUnit InstantiateChainy()
    {
        return nodeFactory.Instantiate<ChainyUnit>(ResourceLoader.Load<PackedScene>("res://Units/chainy.tscn"), levelRoot);
    }
}

public sealed class TestAction : TileAction
{
    public override GDTask Perform()
    {
        GD.Print("sdkfjslkdfjlk");
        return GDTask.CompletedTask;
    }
}