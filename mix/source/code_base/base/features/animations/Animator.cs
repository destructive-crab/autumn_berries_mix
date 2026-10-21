namespace mix.gameplay.systems;

public struct AnimatorCondition
{
    public string Id;
    public bool Value;

    public AnimatorCondition(string id, bool value)
    {
        Id = id;
        Value = value;
    }
}
public struct StateTransition 
{
    public bool Immediate;

    public AnimatorCondition[] Conditions;
    public string[] Triggers;

    public string DestinationState;

    public StateTransition(AnimatorCondition[] conditions, string[] triggers, string destinationState, bool immediate = false)
    {
        DestinationState = destinationState;

        Conditions = conditions;
        Triggers = triggers;
        
        Immediate = immediate;
    }
    
    public StateTransition(string[] triggers, string destinationState, bool immediate = false)
    {
        DestinationState = destinationState;
        Triggers = triggers;
        Immediate = immediate;
    }
}

public struct AnimatorState
{
    public string Id;
    public string ClipId;
    public List<StateTransition> Transitions;

    public AnimatorState(string clipId, StateTransition[] transitions)
    {
        ClipId = clipId;
        Transitions = new(transitions);
    }

    public AnimatorState(string id)
    {
        Id = id;
    }
}

public struct Animator
{
    public AnimatorState State;

    public Dictionary<string, AnimatorState> States;
    public Dictionary<string, bool> Conditions;
    public List<string> Triggers;

    public AnimationsContainer Animations;
}

