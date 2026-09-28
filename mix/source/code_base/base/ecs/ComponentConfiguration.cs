namespace destruct.ecs;

public delegate void ComponentConfiguration<T>(ref T component) where T : struct;