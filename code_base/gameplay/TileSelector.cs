using System;
using System.Collections.Generic;
using System.Linq;
using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using Godot;

namespace autumn_berries_mix.Level;

public sealed class TileSelector : ITileSelector 
{
    private Vector2I offset;

    private Vector2I currentPointingTile;
    private Vector2I currentSelectedTile;
    
    private readonly Queue<int> selectionQueue = new();

    private ILevelInputService levelInputService;
    
    private GameMap map;
    private ECSWorld world;
    private bool cursorOutsideOfMapBounds;

    [Inject]
    private void Construct(ILevelInputService levelInputService, GameMap map, ECSWorld world)
    {
        this.levelInputService = levelInputService;
        this.map = map;
        this.world = world;

        levelInputService.OnTileSelected += OnTileSelectedInput;
    }

    public void Process()
    {
        Vector2 mousePos = levelInputService.MousePosition;
        var pointingTilePosition = new Vector2I(
            (int)(mousePos.X / Global.TILE_SIZE),
            (int)(mousePos.Y / Global.TILE_SIZE));

        cursorOutsideOfMapBounds = !map.Contains(pointingTilePosition);
        if (cursorOutsideOfMapBounds) return;
        
        PointOnTile(pointingTilePosition);
    }

    private void PointOnTile(Vector2I tilePosition)
    {
        if (tilePosition != currentPointingTile)
        {
            currentPointingTile = tilePosition;
            IEnumerable<int> cellTiles = map.Tiles(tilePosition);
            world.Pool<Pointed>().New(cellTiles.Last());
        }
    }

    private void OnTileSelectedInput()
    {
        if (cursorOutsideOfMapBounds) return;

        if (!SelectNextTile())
        {
            selectionQueue.Clear();
            IEnumerable<int> cellTiles = map.Tiles(currentPointingTile);
            foreach (var entityId in cellTiles)
            {
                selectionQueue.Enqueue(entityId);
            }

            currentSelectedTile = currentPointingTile;
            SelectNextTile();
        }
    }

    private bool SelectNextTile()
    {
        if (currentSelectedTile != currentPointingTile) return false;
        if (selectionQueue.TryDequeue(out int entityId))
        {
            LGR.PM(entityId);
            if (!world.Pool<Selected>().Has(entityId))
            {
                world.Pool<Selected>().New(entityId);
            }
            return true;
        }

        return false;
    }
}