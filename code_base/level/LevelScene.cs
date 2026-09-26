using System;
using System.Collections.Generic;
using autumn_berries_mix.Level;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destructive_code.Tilemap;
using destructive_code.UI;
using Godot;

namespace destruct;

public struct Terrain
{
    public int SourceId;
    public Vector2I AtlasPosition;

    public Terrain(int sourceId, Vector2I atlasPosition)
    {
        SourceId = sourceId;
        AtlasPosition = atlasPosition;
    }
}

public struct TerrainChanges {}

public struct Selectable
{
    public Func<TileAction[]> TileActions;
}

public struct Selected {}
public struct Pointed {}
public struct ArrowOverlay
{
    public Cell Cell;
    public Vector2I Direction;
}

public struct PlayerUnit
{
    public string Name;
}

public struct NodeTile
{
    public PackedScene PackedScene;
}

public struct TileCellAttachmentChange { }

public sealed class ActionData
{
    public Texture2D Icon;
    public Texture2D SelectedIcon;
}

public abstract class TileAction
{
    public ActionData Data { get; protected set; }
    
    public abstract void Select();
    public abstract void Deselect();
}

public partial class LevelScene : Scene, IDependenciesProvider
{
    private ECSWorld          world;
    
    private GameMap           map;
    private TileMapLayer      layer;
    private LevelInputService inputService;
    private TileSelector      tileSelector;
    
    public override IDependenciesProvider[] GetProviders() => [this];

    protected override void OnSceneLoaded()
    {
        tileSelector = Game.G<DIKernel>().InjectWithBase(new TileSelector());
        
        world.Pool<NodeTile>().OnNew += CreateNodeTile;

        AddChild(Game.G<INodeFactory>().InjectNodeTree(GD.Load<PackedScene>("res://ui/GameHUD.tscn").Instantiate<GameHUD>()));
        
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                Entity cellEntity = world.New();

                ref Cell cell = ref world
                    .Pool<Cell>()
                    .NewComponent(cellEntity.Id, (ref Cell cell) =>
                    {
                        cell.Position = new(x, y);
                    });
                
                Entity tileEntity = world.New();
                int tile = map.NewTileOn(new Vector2I(x, y));
                ref Terrain terrain = ref world.Pool<Terrain>().NewComponent(tile, (ref Terrain terrain) =>
                {
                    terrain.SourceId = 0;
                    terrain.AtlasPosition = new Vector2I(0, 0);
                });
            }
        }

        CreateChainy();

        Systems.Register(new OverlaySystem(this, world));
    }

    public override void _Process(double delta)
    {
        tileSelector?.Process();
    }

    private int chainyId = 0;
    public void CreateChainy()
    {
        chainyId = map.NewTileOn(0, 0);
        
        world.Pool<PlayerUnit>().NewComponent(chainyId, (ref PlayerUnit unitComponent) =>
        {
            unitComponent.Name = "Chainy";
        });
        
        world.Pool<NodeTile>().NewComponent(chainyId, (ref NodeTile nodeTileComponent) =>
        {
            nodeTileComponent.PackedScene = GD.Load<PackedScene>("res://units/chainy.tscn");
        });
        
        world.Pool<Selectable>().NewComponent(chainyId, (ref Selectable selectableComponent) =>
        {
            selectableComponent.TileActions = GetTestActions;
        });
    }

    private TileAction[] testActions = null;
    private TileAction[] GetTestActions()
    {
        if (testActions == null)
        {
            testActions = [Game.G<DIKernel>().InjectWithBase(new TileMoveAction(new(0, 0), chainyId))];
        }
        return testActions;
    }

    private void UpdateTilePosition(EntityComponentPool<TileCellAttachmentChange> pool, int tileId)
    {
        
    }

    private void CreateNodeTile(EntityComponentPool<NodeTile> pool, int tileId)
    {
        var a = pool.Get(tileId);
        var cell = world.Pool<Cell>().Get(world.Pool<Tile>().Get(tileId).AttachedToCell);
        var instance = a.PackedScene.Instantiate<Node2D>();
        instance.Name = "Node Tile " + tileId;
        instance.Position = Global.TILE_SIZE * cell.Position + new Vector2I(Global.TILE_SIZE / 2, Global.TILE_SIZE / 2);
        AddChild(instance);
    }

    private void RemoveNodeTiles(EntityComponentPool<NodeTile> pool, int tileId)
    {
        
    }
    
    [Provide(true)]
    private ECSWorld ProvideECSWorld()
    {
        world = new ECSWorld();
        return world;
    }
    
    [Provide(true)]
    private GameMap ProvideGameMap()
    {
        map = new GameMap();
        return map;
    }
    
    [Provide(true)]
    private ILevelInputService ProvideInputService()
    {
        inputService = new LevelInputService();
        AddChild(inputService);
        return inputService;
    }
}

public sealed class TileMoveAction : TileAction
{
    [Inject] private ECSWorld world;
    [Inject] private GameMap  gameMap;

    private readonly Vector2I originalPosition;
    private List<Vector2I> availableTiles = new();

    private int entityId;

    public TileMoveAction(Vector2I originalPosition, int entityToTransit)
    {
        this.originalPosition = originalPosition;

        Data = new();
        Data.Icon = GD.Load<Texture2D>("res://art/UI/AbilitiesHUD/move_idle.tres");
        Data.Icon = GD.Load<Texture2D>("res://art/UI/AbilitiesHUD/move_selected.tres");

        entityId = entityToTransit;
    }

    public override void Select()
    {
        world.Pool<Selected>().OnNew += OnTileSelected;

        Cell[] connections = gameMap.GetConnectionsOf(originalPosition);
        foreach (var connection in connections)
        {
            world.Pool<ArrowOverlay>().NewComponent(world.New(), (ref ArrowOverlay c) =>
            {
                c.Cell = connection;
                c.Direction = connection.Position - originalPosition;
            });
            availableTiles.Add(connection.Position);
        }
    }

    private void OnTileSelected(EntityComponentPool<Selected> pool, int entityId)
    {
        if (world.Pool<Tile>().Has(entityId))
        {
            Cell cell = world.Pool<Cell>().Get(gameMap.CellFromTile(entityId));
            if (availableTiles.Contains(cell.Position))
            {
                gameMap.TransitTile(entityId, cell.Position);
            }
        }
    }

    public override void Deselect()
    {
    }
}
