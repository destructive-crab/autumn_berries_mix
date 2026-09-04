using System.Collections.Generic;
using destructive_code.Debug;
using destructive_code.Debug.Units;
using destructive_code.DI;
using destructive_code.Tilemap;
using Godot;

namespace destructive_code.Level;

public enum LevelLayers {
    TerrainLayer,
    ObstacleLayer,
    UnitsLayer,
    BottomOverlayLayer,
    TopOverlayLayer,
}

public sealed class MapProcessor
{
    private IInputService inputService;
    
    private GameMap<LevelLayers> map;
    private Vector2I offset;

    private Vector2I currentSelectedTile;
    
    private List<ISelectableTile> currentSelectionQueue = new();
    private int selectionQueuePosition = 0;

    public MapProcessor(IInputService inputService, GameMap<LevelLayers> map)
    {
        this.inputService = inputService;
        this.map = map;
        inputService.OnTileSelected += OnTileSelected;
    }

    private void OnTileSelected()
    {
        if (selectionQueuePosition > 0)
        {
            selectionQueuePosition--;
            currentSelectionQueue[selectionQueuePosition].Select();
            return;
        }
        
        Vector2 mousePos = inputService.MousePosition;

        currentSelectedTile = new((int)mousePos.X / Global.TILE_SIZE, (int)mousePos.Y / Global.TILE_SIZE);

        currentSelectionQueue.Clear();
        
        ITile[] tiles = map.TilesAt(currentSelectedTile);
        
        for (int i = 0; i < tiles.Length; i++)
        {
            ITile tile = tiles[i];
            
            if (tile is ISelectableTile selectableTile)
            {
                currentSelectionQueue.Add(selectableTile);
            }
        }

        selectionQueuePosition = currentSelectionQueue.Count - 1;
        
        if (selectionQueuePosition >= 0)
        {
            currentSelectionQueue[selectionQueuePosition].Select();
        }
    }
}

public partial class LevelRoot : Node2D, IDependenciesProvider
{
    [Export] public TileSet TerrainTileSet;
    
    private GameMap<LevelLayers> map;

    [Inject] private IInputService inputService;

    private readonly TileAlias AutumnGrassTile = new TileAlias(0, new Vector2I(0,0));
    private MapProcessor processor;
    
    public override void _Ready()
    {
        GameRoot.I.DIKernel.InjectWithBase(this);
        GameRoot.I.DIKernel.RegisterLocalDependencies(this);
        
        map = new GameMap<LevelLayers>(20, 20);

        TerrainTileMapLayer terrain = new TerrainTileMapLayer();
        terrain.TileSet = TerrainTileSet;
        AddChild(terrain);
        
        map.LocalServices.Register(terrain);
        var a = new UnitsFactory();
        GameRoot.I.DIKernel.InjectWithBase(a);
        map.LocalServices.Register(a);
        
        map.AddLayer(LevelLayers.TerrainLayer);
        map.AddLayer(LevelLayers.BottomOverlayLayer);
        map.AddLayer(LevelLayers.ObstacleLayer);
        map.AddLayer(LevelLayers.TopOverlayLayer);

        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                map.L(LevelLayers.TerrainLayer).PlaceTileAt(new TerrainTile(AutumnGrassTile), x, y);
            }
        }

        map.L(LevelLayers.TerrainLayer).PlaceTileAt(new TerrainTile(AutumnGrassTile), 0,0);
        
        map.L(LevelLayers.ObstacleLayer).PlaceTileAt(new ChainyTile(), 0,0);
        
        processor = new MapProcessor(inputService, map);
    }

    public override void _Process(double delta)
    {
        
        
    }

    [Provide(true)]
    private LevelRoot ProvideLevelRootForDI() => this;
}