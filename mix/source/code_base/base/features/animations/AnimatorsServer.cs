using destruct.Debug;
using destruct.ecs;
using destruct.ServiceLocators;

namespace mix.gameplay.systems;

public struct AnimationsContainerReference
{
    public AnimationsContainer Container;
}

public sealed class SpriteAnimationState : AnimatorState
{
    private readonly StateTransition[] transitions;
    private readonly string ClipName;

    public SpriteAnimationState(StateTransition[] transitions, string clipName)
    {
        this.transitions = transitions;
        this.ClipName = clipName;
    }

    public override void Begin(Animator animator)
    {
        animator.World
            .GetFrom<SpriteAnimation>(animator.Entity)
            .SetClip(animator.World.GetFrom<AnimationsContainerReference>(animator.Entity).Container.Get(ClipName));
    }

    public override AnimatorState? Update(Animator animator)
    {
        foreach (StateTransition transition in transitions)
        {
            if (transition.Validate(animator))
                return transition.DestinationState;
        }

        return null;
    }

    public override void End(Animator animator)
    {
    }
}

public struct TransitionCondition
{
    public string Id;
    public bool Condition;

    public bool Equals(TransitionCondition other)
    {
        return Id == other.Id && Condition == other.Condition;
    }

    public override bool Equals(object? obj)
    {
        return obj is TransitionCondition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Condition);
    }
}

public sealed class StateTransition
{
    public readonly bool Immediate;
    
    public readonly List<string>              RequiredTriggers = new();
    public readonly List<TransitionCondition> RequiredConditions = new();

    public AnimatorState DestinationState;
    
    public bool Validate(Animator animator)
    {
        foreach (string trigger in RequiredTriggers)
        {
            if (!animator.HasTrigger(trigger))
                return false;
        }
        
        foreach (var condition in RequiredConditions)
        {
            if (animator.GetCondition(condition.Id) != condition.Condition)
                return false;
        }

        return true;
    }
}

public abstract class AnimatorState
{
    public abstract void Begin(Animator animator);
    public abstract AnimatorState? Update(Animator animator);
    public abstract void End(Animator animator);
}

public sealed class Animator
{
    public ECSWorld World { get; private set; }
    public int Entity { get; private set; }
    public AnimatorState State { get; private set; }

    public readonly ServiceLocator<Object> ServiceLocator = new(); 
    
    private readonly List<AnimatorState> states = new();

    private readonly Dictionary<string, bool> currentConditions = new();
    private readonly HashSet<string> currentTriggers = new();

    public bool GetCondition(string id)
    {
        if (currentConditions.TryGetValue(id, out bool value))
        {
            return value;
        }

        LGR.PERR($"Animator has no condition {id}");
        return false;
    }

    public bool HasTrigger(string id)
    {
        return currentTriggers.Contains(id);
    }

    public void Trigger(string id)
    {
        currentTriggers.Add(id);
    }

    public void SetCondition(string id, bool value)
    {
        if (!currentConditions.ContainsKey(id))
        {
            LGR.PERR($"Animator has no condition {id}");
            return;
        }
        
        currentConditions[id] = value;
    }
    
    public void UpdateState()
    {
        if (State == null) return;
        
        AnimatorState? nextState = State.Update(this);
        
        if (nextState != null)
        {
            State.End(this);
            State = nextState;
            nextState.Begin(this);
        }
    }
}

public class AnimatorsServer
{
    private readonly List<Animator> freeAnimators = new();
    private readonly List<Animator> currentAnimators = new();
    
    public Animator GetNewAnimator()
    {
        if (freeAnimators.Count > 0)
        {
            Animator animatorFromPool = freeAnimators[0];
            freeAnimators.RemoveAt(0);
            
            currentAnimators.Add(animatorFromPool);
            return animatorFromPool;
        }

        var newAnimator = new Animator();
        currentAnimators.Add(newAnimator);
        
        return newAnimator;
    }

    public void FreeAnimator(Animator animator)
    {
        if (!currentAnimators.Remove(animator))
        {
            throw new Exception("Tried to free unused animator");
        }
        freeAnimators.Add(animator);
    }

    public void UpdateAnimators()
    {
        for (var i = 0; i < currentAnimators.Count; i++)
        {
            currentAnimators[i].UpdateState();
        }
    }
}