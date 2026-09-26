using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using destruct.Debug;
using destruct.DI;
using Godot;

namespace destruct.ecs;

public struct Entity
{
    public readonly int Id;
    public readonly ECSWorld Owner;

    public Entity(int id, ECSWorld owner)
    {
        Id = id;
        Owner = owner;
    }
}

public sealed class CellComponentPools
{
    public static int TypesCount = 1;
}

public delegate void ComponentConfiguration<T>(ref T component) where T : struct;

public sealed class EntityComponentPool<TComponent> : IEntityComponentPool
    where TComponent : struct
{
    public Type Type { get; }
    
    
    private TComponent[] components;
    private int[] entityToComponent;
    private readonly Queue<int> freeComponents = new();

    private int entitiesCapacity;
    private int poolCapacity;

    private int componentsCount = 0;

    public event Action OnChanged;
    
    public event Action<EntityComponentPool<TComponent>, int> OnNew;
    public event Action<EntityComponentPool<TComponent>, int> OnRemoved;
    
    public event Action<EntityComponentPool<TComponent>, int> OnDirty;

    private readonly ECSWorld world;

    public EntityComponentPool(int poolCapacity, int entitiesCapacity, ECSWorld world)
    {
        Type = typeof(TComponent);
        
        this.entitiesCapacity = entitiesCapacity;
        this.world = world;
        this.poolCapacity = poolCapacity;

        components = new TComponent[poolCapacity];
        entityToComponent = new int[entitiesCapacity];
    }

    private int GetNewComponentIndexFor(int entityId)
    {
        if (entityToComponent.Length <= entityId)
        {
            entitiesCapacity *= 2;
            Array.Resize(ref entityToComponent, entitiesCapacity);
        }
        
        if (entityToComponent[entityId] != 0)
            throw new Exception($"Component already added to {entityId}");
        
        int index = -1;
        if (freeComponents.Count > 0)
        {
            index = freeComponents.Dequeue();
        }
        else
        {
            componentsCount++;
            index = componentsCount;

            if (index >= components.Length)
            {
                poolCapacity *= 2;
                Array.Resize(ref components, poolCapacity);
                LGR.PM($"Pool capacity increased {poolCapacity} {components.Length} for index {index}");
            }
        }

        return index;
    }
    
    public int New(int entityId)
    {
        int index = GetNewComponentIndexFor(entityId);
        
        entityToComponent[entityId] = index;
        
        OnNew?.Invoke(this, entityId);
        OnChanged?.Invoke();

        world.AddComponentFor(entityId, TileComponent<TComponent>.TypeIndex);
        
        return index;
    }

    public bool Has(int entityId)
    {
        return entityToComponent.Length > entityId && entityToComponent[entityId] != 0;
    }
    
    public void Remove(int entityId)
    {
        components[entityToComponent[entityId]] = default;
        freeComponents.Enqueue(entityToComponent[entityId]);
        entityToComponent[entityId] = 0;
        
        world.RemoveComponentFor(entityId, TileComponent<TComponent>.TypeIndex);
        OnRemoved?.Invoke(this, entityId);
    }
    
    public ref TComponent NewComponent(int entityId, ComponentConfiguration<TComponent> configuration = null)
    {
        int index = GetNewComponentIndexFor(entityId);
        
        entityToComponent[entityId] = index;
        configuration?.Invoke(ref components[index]);
        
        OnNew?.Invoke(this, entityId);
        OnChanged?.Invoke();

        world.AddComponentFor(entityId, TileComponent<TComponent>.TypeIndex);
        
        return ref components[index];
    }
    
    public ref TComponent NewComponent(Entity entity, ComponentConfiguration<TComponent> configuration = null)
        => ref NewComponent(entity.Id, configuration);

    public object GetItem(int entityId)
    {
        return components[entityToComponent[entityId]];
    }

    public ref TComponent Get(int entityId)
    {
        if (entityToComponent[entityId] == 0)
            throw new Exception($"No component attached to {entityId}");

        return ref components[entityToComponent[entityId]];
    }
    
    public void MarkAsDirty(int entityId)
    {
        OnDirty?.Invoke(this, entityId);
    }
}

public struct TileComponent<TComponent> 
    where TComponent : struct
{
    public static readonly int TypeIndex;
    public static readonly Type Type;

    static TileComponent()
    {
        TypeIndex = Interlocked.Increment(ref CellComponentPools.TypesCount);
        Type = typeof(TComponent);
    }
}

public struct EntityData
{
    public ECSWorld World;
    public readonly List<int> Components = new();

    public EntityData()
    {
        World = null;
        Components = new();
    }
}

public interface IMatrixElement
{
    public Vector2I Position { get; set; }
}


public struct Cell
{
    public Vector2I Position;
}

public struct Tile
{
    public int AttachedToCell = -1;

    public Tile() { }
}

public struct GameMapLayoutUpdate
{
    public int[] UpdatedCells;
}

public sealed class GameMap
{
    //cell position to cell entity id
    private readonly Dictionary<Vector2I, int> positionToCell = new();
    //cell entity id to tile entity ids of attached tiles
    private readonly Dictionary<int, List<int>> cellToTiles = new();
    //tile entity id to cell entity id 
    private readonly Dictionary<int, int> tileToCell = new();
    
    private ECSWorld world;

    [Inject]
    private void Inject(ECSWorld world)
    {
        this.world = world;

        world.Pool<Cell>().OnNew     += OnNew;
        world.Pool<Cell>().OnRemoved += OnRemoved;
    }

    public int NewTileOn(int cellX, int cellY) => NewTileOn(new(cellX, cellY));
    
    public int NewTileOn(Vector2I cellPosition)
    {
        if (!positionToCell.ContainsKey(cellPosition))
            throw new Exception($"No cell on position {cellPosition} found");

        Entity newId = world.New();
        
        world.Pool<Tile>().NewComponent(newId.Id, Configuration);

        void Configuration(ref Tile component)
        {
            var cell = positionToCell[cellPosition];
            component.AttachedToCell = cell;
            
            if (cellToTiles.ContainsKey(cell))
            {
                cellToTiles[cell].Add(newId.Id);
            }
            else
            {
                cellToTiles.Add(cell, new([newId.Id]));
            }
            
        }

        return newId.Id;
    }

    public Cell Cell(Vector2I position)
    {
        return world.Pool<Cell>().Get(positionToCell[position]);
    }
    
    public IEnumerable<int> Tiles(Vector2I tilePosition)
    {
        return cellToTiles[positionToCell[tilePosition]];
    }

    public int CellFromTile(int tileEntityId)
    {
        return tileToCell[tileEntityId];
    }
        
    public void TransitTile(int entityId, Vector2I newPosition)
    {
        EntityComponentPool<Tile> pool = world.Pool<Tile>();
        
        if (!pool.Has(entityId))
        {
            throw new Exception($"Entity {entityId} is not a tile");
        }
        
        int currentCell = pool.Get(entityId).AttachedToCell;
        cellToTiles[currentCell].Remove(entityId);
        cellToTiles[positionToCell[newPosition]].Add(entityId);
        
    }
    
    private void OnNew(EntityComponentPool<Cell> pool, int entityId)
    {
        ref var cell = ref pool.Get(entityId);

        if (!positionToCell.TryAdd(cell.Position, entityId))
        {
            throw new Exception($"Cell on {cell.Position} already created");
        }
    }

    private void OnRemoved(EntityComponentPool<Cell> pool, int entityId)
    {
        positionToCell.Remove(pool.Get(entityId).Position);
    }

    public bool Contains(Vector2I position)
    {
        return positionToCell.ContainsKey(position);
    }

    public Cell[] GetConnectionsOf(Vector2I originalPosition)
    {
        var offsets = UTL.GetAllOffsets();
        List<Cell> result = new();

        foreach (var offset in offsets)
        {
            var cellPosition = originalPosition + offset;
            if (Contains(cellPosition))
            {
                result.Add(Cell(cellPosition));
            }
        }

        return result.ToArray();
    }
}

public sealed class MapFilter
{
    public IEnumerable<Entity> Tiles => tiles;
    private readonly List<Entity> tiles = new();

    public Rect2I IncludedArea { get; private set; } = new(0, 0, 0, 0);
    public Rect2I ExcludedArea { get; private set; } = new(0, 0, 0, 0);

    public IEnumerable<int> IncludedComponents => includedComponents;
    private readonly List<int> includedComponents = new();
    
    public IEnumerable<int> ExcludedComponents => excludedComponents;
    private readonly List<int> excludedComponents = new();

    public MapFilter IncludeArea(Rect2I area)
    {
        IncludedArea = area;
        return this;
    }

    public MapFilter ExcludeArea(Rect2I area)
    {
        ExcludedArea = area;
        return this;
    }

    public MapFilter IncludeComp<TComponent>()
        where TComponent : struct
    {
        includedComponents.Add(TileComponent<TComponent>.TypeIndex);
        return this;
    }
    

    public MapFilter ExcludeComp<TComponent>()
        where TComponent : struct
    {
        excludedComponents.Add(TileComponent<TComponent>.TypeIndex);
        return this;
    }
    
    public bool IsCompatible(ECSWorld world, int entityId)
    {
        EntityData data = world.GetData(entityId);
        
        foreach (int component in includedComponents)
        {
            if (!data.Components.Contains(component)) 
                return false;
        }
        
        foreach (int component in excludedComponents)
        {
            if (data.Components.Contains(component)) 
                return false;
        }
        
        return true;
    }

    public override bool Equals(object obj)
    {
        return obj is MapFilter mapFilter &&
                   mapFilter.IncludedComponents == IncludedComponents &&
                   mapFilter.ExcludedComponents == ExcludedComponents &&
                   mapFilter.IncludedArea == IncludedArea &&
                   mapFilter.ExcludedArea == ExcludedArea;

    }
}

public interface IEntityComponentPool
{
    Type Type { get; }
    
    object GetItem(int index);
    int New(int entityId);
    void Remove(int entityId);
}