using destruct;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using destruct.postprocessing;
using mix.inputs;

namespace destruct.gameplay.systems;

public struct Selected { }
public struct Selectable { }

public sealed class SelectionSystem : ISystem, IInputsProcessSystem
{
    public string DebugName() => nameof(SelectionSystem);

    private ECSWorld world; 
    
    private IGeneralInputService inputService;
    
    private EntityComponentPool<Selected> selectedPool;

    private readonly List<int> hoveredEntitiesSorted = new();
    private int currentSelected = -1;

    [Inject]
    private void Inject(ECSWorld world, IGeneralInputService levelInputService)
    {
        this.world = world;
        selectedPool = world.Pool<Selected>();
        world.Filter()
            .In<Hovered>()
            .In<Selectable>()
            .Subscribe((e) =>
            {
                hoveredEntitiesSorted.Clear();
                hoveredEntitiesSorted.AddRange(e);
                hoveredEntitiesSorted.Sort((a,b) => UTL.CompareZOrderInPosition(a,b,world));
            });

        inputService = levelInputService;
    }
    
    public void ProcessInputs(bool guiWasOverlapped)
    {
        if (guiWasOverlapped) return;
        
        if (inputService.LeftMouseButtonDown)
        {
            foreach (int entity in hoveredEntitiesSorted)
            {
                if (currentSelected != -1)
                {
                    selectedPool.Remove(currentSelected);
                }
                selectedPool.New(entity);
                currentSelected = entity;
                return;
            }
        }
    }
}