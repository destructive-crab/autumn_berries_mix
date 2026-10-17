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
    
    private InputFieldRuntimeData consoleInput;
    private string currentConsoleString = "";

    private readonly List<string> consoleOutput = new();

    public void DrawGUI()
    {
        if(Raylib.IsKeyPressed(KeyboardKey.F1))
            showConsole = !showConsole;
        
        if (showConsole)
        {
            deGUI.BeginContainer(0, 0, deGUI.ConW(), deGUI.ConH());
            {
                deGUI.FillContainer(new(0,0,0,140));
                
                deGUI.BeginMarginContainer(50);
                {
                    if (deGUI.InputField(currentConsoleString, 0, 0, deGUI.ConW(), ref consoleInput,
                            out currentConsoleString))
                    {
                        ProcessCommand(currentConsoleString);
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
            
            if (tokens[0] == SceneAlias) InvokeMethodIn(Game.CurrentScene, methodName);
            else
            {
                foreach (ISystem system in Game.CurrentScene.Systems.All)
                {
                    if (system.DebugName() == tokens[0])
                    {
                        InvokeMethodIn(system, methodName);
                    }
                }
            }
        }
    }

    private void InvokeMethodIn(object o, string methodName)
    {
        var sceneType = o.GetType();
                
        foreach (MethodInfo info in sceneType.GetMethods())
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

    public void Hide()
    {
        showConsole = false;
    }
}