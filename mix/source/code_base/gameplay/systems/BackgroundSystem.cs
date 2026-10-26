using destruct.scenes;
using Raylib_cs;

namespace mix.code_base.gameplay.systems;

public sealed class BackgroundSystem : IPreRendererSystem, IRendererSystem
{
    public string DebugName() => nameof(BackgroundSystem);
    
    private readonly Shader noiseShader;
    private readonly Shader cloudsShader;
    
    private RenderTexture2D backgroundShaderNoiseTexture;
    private Vec2I currentWindowSize;
    
    private int timeLoc;
    
    private bool enabled = false;

    public BackgroundSystem()
    {
        noiseShader = Assets.Shaders.Load("assets/shaders/background/noise.fs");
        cloudsShader = Assets.Shaders.Load("assets/shaders/background/clouds.fs");
        
        timeLoc = Raylib.GetShaderLocation(noiseShader, "u_time");
        UpdateScreenSize();
    }

    public void PreRenderer()
    {
        if (!enabled) return;
        
        if (currentWindowSize.X != Raylib.GetScreenWidth() ||
            currentWindowSize.Y != Raylib.GetScreenHeight())
        {
            UpdateScreenSize();
        }
        
        float time = 0.8f * (float)Raylib.GetTime();
        Raylib.SetShaderValue(noiseShader, timeLoc, time, ShaderUniformDataType.Float);
        
        Raylib.BeginShaderMode(noiseShader);
        {
            Raylib.BeginTextureMode(backgroundShaderNoiseTexture);
            {
                F(backgroundShaderNoiseTexture.Texture.Width, backgroundShaderNoiseTexture.Texture.Height);
            }
            Raylib.EndTextureMode();
        }
        Raylib.EndShaderMode();
    }

    private int mode = 0;
    public void Render()
    {
        if (!enabled) return;
        
        Raylib.BeginShaderMode(cloudsShader);
        {
            Rlgl.SetTexture(backgroundShaderNoiseTexture.Texture.Id);
            {
                F(currentWindowSize.X, currentWindowSize.Y);
            }
            Rlgl.DisableTexture();
        }
        Raylib.EndShaderMode();
        
        if (mode == 0) Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color(160, 35, 75, 140));
        if (mode == 1) Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color(9, 2, 51, 140));
        if (mode == 2) Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), new Color(18, 1, 117, 140));
        if (mode == 3) return;
    }

    private void F(float w, float h)
    {
        Rlgl.Begin(DrawMode.Triangles);
        {
            Rlgl.TexCoord2f(0.0f, 0.0f);
            Rlgl.Vertex2f(0.0f, 0.0f);
    
            Rlgl.TexCoord2f(0.0f, h);
            Rlgl.Vertex2f(0.0f, h);
    
            Rlgl.TexCoord2f(w, 0.0f);
            Rlgl.Vertex2f(w, 0.0f);
    
            Rlgl.TexCoord2f(w, h);
            Rlgl.Vertex2f(w, h);
    
            Rlgl.TexCoord2f(w, 0.0f);
            Rlgl.Vertex2f(w, 0.0f);
    
            Rlgl.TexCoord2f(0.0f, h);
            Rlgl.Vertex2f(0.0f, h);
        }
        Rlgl.End();
    }

    private void UpdateScreenSize()
    {
        currentWindowSize = new(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        backgroundShaderNoiseTexture = Raylib.LoadRenderTexture((int)(MathF.Ceiling(currentWindowSize.X/40f)+8), (int)(MathF.Ceiling(currentWindowSize.Y/40)+8));
    }

    private string Enable()
    {
        enabled = true;
        return "Background shader was enabled";
    }

    private void Disable()
    {
        enabled = false;
    }

    private void NextMode()
    {
        mode++;
        if (mode > 2) mode = 0;
    }
}