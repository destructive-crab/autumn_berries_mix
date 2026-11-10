namespace destruct.scenes;

public interface IInputsProcessSystem : ISystem
{
    void ProcessInputs(bool guiWasOverlapped);
}