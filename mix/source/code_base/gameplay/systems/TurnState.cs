using destruct;
using destruct.DI;
using destruct.ecs;

namespace mix.code_base.gameplay.systems;

public interface IPlayerTurnSystem
{
    void Begin(PlayerTurn turn);
    void End(PlayerTurn turn);
}
    
public sealed class TurnState 
{
    public Turn CurrentTurn { get; private set; }

    public async void StartTurnCycle(Turn turn)
    {
        CurrentTurn = turn;

        while (Game.Awake)
        {
            if (CurrentTurn is PlayerTurn)
            {
                await CurrentTurn.Perform();
                CurrentTurn = Game.G<DIKernel>().InjectWithBase(new EnemyTurn());
            }
            else if (CurrentTurn is EnemyTurn)
            {
                await CurrentTurn.Perform();
                CurrentTurn = new PlayerTurn();
            }
        }
    }
}

public abstract class Turn
{
    public abstract Task Perform();
}

public sealed class PlayerTurn : Turn
{
    public int PointsLeft { get; private set; } = 3;

    public void RemovePoints(int points)
    {
        PointsLeft -= points;
    }

    public override async Task Perform()
    {
        while (PointsLeft > 0)
        {
            await Task.Yield();
        }
    }
}

public sealed class EnemyTurn : Turn
{
    [Inject] private ECSWorld world;
    [Inject] private GameplayFilters filters;
    
    public override async Task Perform()
    {
        foreach (int entity in filters.EnemyUnits.Entities)
        {
            
        }
    }
}
