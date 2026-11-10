using Raylib_cs;

namespace mix.gameplay.systems;

public sealed class VignetteMinimal : PostprocessingOverride
{
    private Shader shader;

    private const string SHADER_ASSET_PATH = "assets/shaders/postprocessing/vignette.fs";

    private const string INTENSITY_NAME  = "intensity";
    
    private int IntensityLocation;
    
    public VignetteMinimal SetIntensity(float intensity)
    {
        Raylib.SetShaderValue(shader, IntensityLocation, [intensity], ShaderUniformDataType.Float);
        return this;
    }
    
    public override void Load()
    {
        shader = Assets.Shaders.Load(SHADER_ASSET_PATH);

        IntensityLocation  = Raylib.GetShaderLocation(shader, INTENSITY_NAME);
    }

    public override void Unload()
    {
        Assets.Shaders.Unload(SHADER_ASSET_PATH);
    }

    public override void Defaults()
    {
        SetIntensity(0.1f);
    }

    protected override ref Shader GetShader() => ref shader;
}