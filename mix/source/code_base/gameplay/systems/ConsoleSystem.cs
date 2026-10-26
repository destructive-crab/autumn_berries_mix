using System.Reflection;
using destruct;
using destruct.imgui;
using destruct.scenes;
using Raylib_cs;

namespace mix.code_base.gameplay.systems;

public sealed class ConsoleSystem : IGUISystem
{
    public string DebugName() => nameof(ConsoleSystem);

    private bool showConsole;
    
    public const string SceneAlias = "scene";
    public const string WorldAlias = "world";
    
    private InputFieldRuntimeData consoleInput;
    private string currentConsoleString = "";
    private string previousConsoleString = "";

    private readonly List<string> consoleOutput = new();
    
    private const BindingFlags BindingFlags =
        System.Reflection.BindingFlags.Instance |
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic;

    public void DrawGUI()
    {
        if(Raylib.IsKeyPressed(KeyboardKey.F1))
            showConsole = !showConsole;
        
        if (showConsole)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Up))
                currentConsoleString = previousConsoleString;
            
            deGUI.BeginContainer(0, 0, deGUI.ConW(), deGUI.ConH());
            {
                deGUI.FillContainer(new(0,0,0,140));
                
                deGUI.BeginMarginContainer(50);
                {
                    if (deGUI.InputField(currentConsoleString, 0, 0, deGUI.ConW(), ref consoleInput,
                            out currentConsoleString))
                    {
                        ProcessCommand(currentConsoleString);
                        previousConsoleString = currentConsoleString;
                        currentConsoleString = "";
                    }

                    deGUI.BeginContainer(0, 50, deGUI.ConW(), deGUI.ConH() - 50);
                    {
                        deGUI.ContainerBorder();

                        foreach (string s in consoleOutput)
                        {
                            deGUI.Text(s, 20, 10, deGUI.NextY(), Color.White);
                            deGUI.YSpace(5);
                        }
                    }
                    deGUI.EndContainer();
                }
                deGUI.EndGUI();
            }
            deGUI.EndGUI();
        }
    }

    private void ProcessCommand(string command)
    {
        string[] tokens = command.Split('.');
        if (tokens.Length == 2)
        {
            string methodName = tokens[1];

            if (tokens[0] == SceneAlias)
            {
                InvokeMethod(Game.CurrentScene, methodName);
            }

            if (tokens[0] == WorldAlias && Game.CurrentScene is ECSScene ecsWorld)
            {
                InvokeMethod(ecsWorld.World, methodName);
            }
            else
            {
                foreach (ISystem system in Game.CurrentScene.Systems.All)
                {
                    if (system.DebugName() == tokens[0])
                    {
                        InvokeMethod(system, methodName);
                    }
                }
            }
        }
    }

    private void InvokeMethod(object o, string methodName)
    {
        var sceneType = o.GetType();
                
        foreach (MethodInfo info in sceneType.GetMethods(BindingFlags))
        {
            if (info.Name == methodName)
            {
                object? output = info.Invoke(o, []);
    
                if (output != null) consoleOutput.Add(output.ToString());
                return;
            }
        }
                
        consoleOutput.Add("Invalid scene method");
    }

    private void Hide()
    {
        showConsole = false;
    }
}