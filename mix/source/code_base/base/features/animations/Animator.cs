using destruct.Debug;

namespace mix.gameplay.systems;

public struct Animator
{
    public string CurrentState;
    
    public HashSet<string> Triggers;
    public Dictionary<string, bool> Conditions;

    public List<AnimatorState> States;
    public AnimationsContainer Container;

    public Animator()
    {
        Triggers = new();
        Conditions = new();
        States = new();
    }

    public Animator(string aseAnimationsAsset, string[] conditions, params AnimatorState[] states)
    {
        Container = Assets.AnimationContainers.Load(aseAnimationsAsset);
        Triggers = new();
        Conditions = new();
        
        foreach (string condition in conditions)
            SetCondition(condition, false);
        
        States = new(states);
    }
    
    public void Trigger(string key)
    {
        Triggers.Add(key);
    }

    public void SetCondition(string key, bool value)
    {
        if (!Conditions.TryAdd(key, value))
        {
            Conditions[key] = value;
        }
    }

    public bool GetCondition(string key)
    {
        if (!Conditions.ContainsKey(key))
        {
            LGR.PERR($"Condition {key} not found");
            return false;
        }
        
        return Conditions[key];
    }

    public bool HasCondition(string condition)
    {
        return Conditions.ContainsKey(condition);
    }
}

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

public struct AnimatorState
{
    public HashSet<string> Triggers;
    public Dictionary<string, bool> Conditions;

    public string ClipName;
    public string Id;

    public AnimatorState(string clipName)
    {
        Id = clipName;
        Triggers = new();
        Conditions = new();
        ClipName = clipName;
    }
    
    public AnimatorState(string id, string clipName)
    {
        Id = id;
        Triggers = new();
        Conditions = new();
        ClipName = clipName;
    }
    
    public AnimatorState()
    {
        Triggers = new();
        Conditions = new();
        Id = "invalid";
        ClipName = "invalid";
    }
    
    public AnimatorState(string[] triggers, AnimatorCondition[] conditions, string clipName)
    {
        Triggers = new(triggers);
        Conditions = new();
        
        foreach (AnimatorCondition condition in conditions)
            Conditions.Add(condition.Id, condition.Value);

        ClipName = clipName;
        Id = ClipName;
    }

    public bool Validate(ref Animator animator)
    {
        foreach (var trigger in Triggers)
        {
            if (!animator.Triggers.Contains(trigger))
                return false;
        }
        
        foreach (var condition in Conditions)
        {
            if (!animator.GetCondition(condition.Key) == condition.Value)
                return false;
        }

        return true;
    }
}