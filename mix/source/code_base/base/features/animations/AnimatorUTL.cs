using System.Runtime.InteropServices;
using destruct.Debug;
using destruct.ecs;

namespace mix.gameplay.systems;

public static class AnimatorUTL
{
    public static ref Animator WithAseAnimations(ref this Animator animator, string assetPath)
    {
        animator.Animations = Assets.AnimationContainers.Load(assetPath);
        return ref animator;
    }
    
    public static ref Animator Trigger(ref this Animator animator, string trigger)
    {
        animator.Triggers.Add(trigger);
        
        return ref animator;
    }
    
    public static ref Animator NewCondition(ref this Animator animator, string condition, bool defaultValue)
    {
        if (animator.Conditions.ContainsKey(condition))
        {
            LGR.PERR($"Condition {condition} already added to animator");
            return ref animator;
        }
        animator.Conditions.Add(condition, defaultValue);
        
        return ref animator;
    }
    
    public static ref Animator SetCondition(ref this Animator animator, string condition, bool value)
    {
        if (!animator.Conditions.ContainsKey(condition))
        {
            LGR.PERR($"No condition {condition} found in animator");
            return ref animator;
        }
        animator.Conditions[condition] = value;
        
        return ref animator;
    }
    
    public static ref Animator AddState(ref this Animator animator, ref AnimatorState state)
    {
        animator.States.Add(state.Id, state);
        return ref animator;
    }

    public static AnimatorState State(string id)
    {
        AnimatorState state = new(id);
        return state;
    }

    public static ref AnimatorState Clip(ref this AnimatorState state, string clipId)
    {
        state.ClipId = clipId;
        return ref state;
    }

    public static ref StateTransition NewTransition(ref this AnimatorState state)
    {
        state.Transitions.Add(new StateTransition());
        ref StateTransition stateTransition = ref CollectionsMarshal.AsSpan(state.Transitions)[^1];
        return ref stateTransition;
    }
    
    public static ref StateTransition TransitionTrigger(ref this StateTransition transition, string triggerId)
    {
        if (transition.Triggers == null)
        {
            transition.Triggers = [triggerId];
        }
        else
        {
            Array.Resize(ref transition.Triggers, transition.Triggers.Length + 1);
            transition.Triggers[^1] = triggerId;
        }
        
        return ref transition;
    }
    
    public static ref StateTransition TransitionCondition(ref this StateTransition transition, string conditionId, bool value)
    {
        if (transition.Conditions == null)
        {
            transition.Conditions = [new(conditionId, value)];
        }
        else
        {
            Array.Resize(ref transition.Conditions, transition.Conditions.Length + 1);
            transition.Conditions[^1] = new(conditionId, value);
        }
        return ref transition;
    }

    public static ref StateTransition TransitionDestination(ref this StateTransition transition, string destinationState)
    {
        transition.DestinationState = destinationState;
        return ref transition;
    }
    
    public static void Pause(ref this SpriteAnimation spriteAnimation)
    {
        spriteAnimation.Paused = true;
    }

    public static void Unpause(ref this SpriteAnimation spriteAnimation)
    {
        spriteAnimation.Paused = false;
    }

    public static bool HasState(ref this Animator animator, string state)
    {
        return animator.States.ContainsKey(state);
    }

    public static bool AnimatorHasState(ECSWorld world, int entityId, string state)
    {
        return world.GetFrom<Animator>(entityId).HasState(state);
    }
}