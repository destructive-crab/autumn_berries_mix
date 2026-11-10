using Raylib_cs;

namespace mix.gameplay.systems;

public abstract class PostprocessingOverride
{
    private RenderTexture2D outputTexture;
    
    public abstract void Load();
    public abstract void Unload();
    public abstract void Defaults();
    
    protected abstract ref Shader GetShader();

    public virtual ref RenderTexture2D PerformRaylibDrawing(ref RenderTexture2D source)
    {
        if (source.Texture.Width != outputTexture.Texture.Width ||
            source.Texture.Height != outputTexture.Texture.Height)
        {
            outputTexture = Raylib.LoadRenderTexture(source.Texture.Width, source.Texture.Height);
        }
        
        Raylib.BeginTextureMode(outputTexture);
        {
            Raylib.BeginShaderMode(GetShader());
            {
                Raylib.DrawTextureRec(source.Texture, new(0,0,source.Texture.Width, -source.Texture.Height), new(0,0), Color.White);
            }
            Raylib.EndShaderMode();
        }
        Raylib.EndTextureMode();

        return ref outputTexture;
    }
}