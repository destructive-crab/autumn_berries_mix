using destruct;
using destruct.debug;
using destruct.DI;
using destruct.ecs;
using destruct.gameplay.systems;
using destruct.postprocessing;
using destruct.renderer;

namespace destruct;

public struct Cell
{
    public Vec2I     Position;
    public List<int> Entities;
}

public static class GameMapUTL
{
    public static bool IsCellWalkable(this GameMap map, Vec2I cell)
    {
        return map.Contains(cell) && !map.CellHasEntityWith<BlockCell>(cell);
    }
}

public sealed class GameMap
{
    public int LayerWidth { get; private set; }
    public int LayerHeight { get; private set; }
    public int CellsCount => LayerWidth * LayerHeight;

    private int[] cells = [];

    [Inject] private ECSWorld world;

    public int   XYToIndex(int x, int y) => UTL.XYToIndex(x, y, LayerWidth, LayerHeight);
    public int   V2IToIndex(Vec2I v2i) => XYToIndex(v2i.X, v2i.Y);
    public Vec2I IndexToV2I(int index) => UTL.IndexToV2I(index, LayerWidth, LayerHeight);

    public void UpdateLayout(IEnumerable<int> entities)
    {
        foreach (int cell in cells)
        {
            world.GetFrom<Cell>(cell).Entities.Clear();
        }

        foreach (int entity in entities)
        {
            if (!world.Pool<Tile>().Has(entity))
            {
                LGR.PW($"Trying to update layout with invalid entity; to tile component on {entity}");
                continue;
            }

            var p = GetCellPositionForTile(entity);
            world.GetFrom<Tile>(entity).CellId = cells[V2IToIndex(p)];
            GetCell(p).Entities.Add(entity);
        }
    }

    public void Create(int width, int height)
    {
        LayerWidth = width;
        LayerHeight = height;
        
        cells = new int[width * height];
        
        world.BlockFiltersEvaluation();
        {
            for (var i = 0; i < cells.Length; i++)
                cells[i] = CreateNewCellOn(IndexToV2I(i));
        }
        world.UnblockFiltersEvaluationAndEvaluate();
    }

    public void Grow(int width, int height)
    {
        LayerWidth += width;
        LayerHeight += height;
        
        int[] newCells = new int[(LayerWidth) * (LayerHeight)];

        for (int i = 0; i < cells.Length; i++)
            newCells[i] = cells[i];

        world.BlockFiltersEvaluation();
        {
            for (int i = cells.Length; i < newCells.Length; i++)
                newCells[i] = CreateNewCellOn(IndexToV2I(i));
        }
        world.UnblockFiltersEvaluationAndEvaluate();
    }
    
    public bool Contains(Vec2I pos)
    {
        return pos.X >= 0 && pos.Y >= 0 && pos.X < LayerWidth && pos.Y < LayerHeight;
    }
    
    public ref Cell GetCell(Vec2I pos)
    {
        ref Cell cell = ref world.Pool<Cell>().Get(GetCellId(pos));
        return ref cell;
    }

    private int GetCellId(Vec2I pos)
    {
        return cells[V2IToIndex(pos)];
    }

    public IEnumerable<int> GetEntities(Vec2I pos)
    {
        return GetCell(pos).Entities;
    }
    
    public IEnumerable<int> GetEntitiesWith<TComponent>(Vec2I pos)
        where TComponent : struct
    {
        List<int> result = new();

        foreach (int entity in GetEntities(pos))
            if (world.HasIn<TComponent>(entity))
                result.Add(entity);

        return result;
    }

    public bool CellHasEntityWith<TComponent>(Vec2I position) 
        where TComponent : struct
    {
        if (!Contains(position))
            throw new Exception($"No cell with position {position} found in map");

        EntityComponentPool<TComponent> pool = world.Pool<TComponent>();
        
        foreach (int entityId in GetEntities(position))
            if (pool.Has(entityId)) return true;

        return false;
    }


    public IEnumerable<Vec2I> GetExistingCellsAroundPositionThat(Vec2I position, Predicate<Vec2I> valid, int radius = 1)
    {
        List<Vec2I> result = new(8);

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x == 0 && y == 0) continue;
                Vec2I cellPosition = position + new Vec2I(x,y);
                
                if (Contains(cellPosition) && valid(cellPosition))
                    result.Add(cellPosition);
            }
        }

        return result;
    }

    public IEnumerable<Vec2I> GetExistingCellsAroundPositionThatHaveEntityWith<TComponent>(Vec2I position,
        int radius = 1)
        where TComponent : struct
        => GetExistingCellsAroundPositionThat(position, CellHasEntityWith<TComponent>);
    
    public IEnumerable<Vec2I> GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<TComponent>(int x, int y)
        where TComponent : struct
        => GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<TComponent>(new(x,y));
    
    public IEnumerable<Vec2I> GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<TComponent>(Vec2I position, int radius = 1)
        where TComponent : struct
        => GetExistingCellsAroundPositionThat(position, c => !CellHasEntityWith<TComponent>(c), radius);
    
    private int CreateNewCellOn(Vec2I position)
    {
        int id = world.New().Id;

        world.Pool<Cell>().New(id, (ref Cell c) =>
        {
            c.Position = position;
            c.Entities = new();
        });
        world.Pool<Position>().New(id, (ref Position p) => p.Set(position.X, position.Y));
        world.Pool<Origin>().New(id, (ref Origin o) => o.Set(0.5f, 0.5f));
        world.Pool<Scale>().New(id, (ref Scale s) => s.Set(1,1));
        
        world.Pool<Selectable>().New(id);
        world.Pool<Hoverable>().New(id);

        return id;
    }

    private Vec2I GetCellPositionForTile(int entityId)
    {
        Position posComponent = world.Pool<Position>().Get(entityId); 
        return posComponent.ToRoundedV2I();
    }

    public void QuitMap(int tileId)
    {
        ref Cell cell = ref world.GetFrom<Cell>(world.GetFrom<Tile>(tileId).CellId);
        
        cell.Entities.Remove(tileId);

        world.GetFrom<Tile>(tileId).CellId = -1;
    }

    public bool IsTileAttachedToCell(int tileId)
    {
        return world.GetFrom<Tile>(tileId).CellId != -1;
    }

    public void TryMoveTileOrAddToCell(int tileId, Vec2I cellPosition)
    {
        if (!Contains(cellPosition))
        {
            DropCellPositionOutOfBoundsError(cellPosition);
            return;
        }
        
        if (IsTileAttachedToCell(tileId))
        {
            MoveTile(tileId, cellPosition);
        }
        else
        {
            GetCell(cellPosition).Entities.Add(tileId);
            
            world.GetFrom<Tile>(tileId).CellId = GetCellId(cellPosition);
        }
    }

    public void AddTileToCell(int tileId, Vec2I cellPosition)
    {
        if (!Contains(cellPosition))
        {
            DropCellPositionOutOfBoundsError(cellPosition);
            return;
        }
        
        if (IsTileAttachedToCell(tileId))
        {
            LGR.PERR($"Cannot add tile to cell {cellPosition} because {tileId} already attached to cell. Use MoveTile");
            return;
        }
        
        GetCell(cellPosition).Entities.Add(tileId);
        
        world.GetFrom<Tile>(tileId).CellId = GetCellId(cellPosition);
    }
    
    public void MoveTile(int tileId, Vec2I newPosition)
    {
        if (!Contains(newPosition))
        {
            DropCellPositionOutOfBoundsError(newPosition);
            return;
        }

        ref Cell cell = ref world.GetFrom<Cell>(world.GetFrom<Tile>(tileId).CellId);
        
        cell.Entities.Remove(tileId);
        
        GetCell(newPosition).Entities.Add(tileId);
        
        world.GetFrom<Tile>(tileId).CellId = GetCellId(newPosition);
    }

    private void DropCellPositionOutOfBoundsError(Vec2I position)
    {
        LGR.PERR($"Cell position is out of bounds of map {position}. Map bounds ({LayerHeight}x{LayerWidth}");
    }
    
    public static bool TryGetCellPosition(ECSWorld world, int entity, out Vec2I position)
    {
        position = Vec2I.Zero;

        if (world.HasIn<Tile>(entity))
        {
            position = world.GetFrom<Cell>(world.GetFrom<Tile>(entity).CellId).Position;
            return true;
        }

        return false;
    }
}