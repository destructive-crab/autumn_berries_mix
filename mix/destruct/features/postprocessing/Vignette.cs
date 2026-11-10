using Raylib_cs;

namespace mix.gameplay.systems;

public sealed class Vignette : PostprocessingOverride
{
    private Shader shader;

    private const string SHADER_ASSET_PATH = "assets/shaders/postprocessing/vignette.fs";

    private const string COLOR_NAME      = "color";
    private const string CENTER_NAME     = "center";
    private const string INTENSITY_NAME  = "intensity";
    private const string SMOOTHNESS_NAME = "smoothness";
    private const string ROUNDNESS_NAME  = "roundness";
    
    private int ColorLocation;
    private int CenterLocation;
    private int IntensityLocation;
    private int SmoothnessLocation;
    private int RoundnessLocation;

    public Vignette SetColor(Color color)
    {
        Raylib.SetShaderValue(shader, ColorLocation, [ color.R/255f, color.G/255f, color.B/255f ], ShaderUniformDataType.Vec3);
        return this;
    }

    public Vignette SetCenter(float x, float y)
    {
        Raylib.SetShaderValue(shader, CenterLocation, [ x, y ], ShaderUniformDataType.Vec2);
        return this;
    }
    
    public Vignette SetIntensity(float intensity)
    {
        Raylib.SetShaderValue(shader, IntensityLocation, [intensity], ShaderUniformDataType.Float);
        return this;
    }
    
    public Vignette SetSmoothness(float smoothness)
    {
        Raylib.SetShaderValue(shader, SmoothnessLocation, [smoothness], ShaderUniformDataType.Float);
        return this;
    }
    
    public Vignette SetRoundness(float roundness)
    {
        Raylib.SetShaderValue(shader, RoundnessLocation, roundness, ShaderUniformDataType.Float);
        return this;
    }
    
    public override void Load()
    {
        shader = Assets.Shaders.Load(SHADER_ASSET_PATH);

        ColorLocation      = Raylib.GetShaderLocation(shader, COLOR_NAME);
        CenterLocation     = Raylib.GetShaderLocation(shader, CENTER_NAME);
        IntensityLocation  = Raylib.GetShaderLocation(shader, INTENSITY_NAME);
        SmoothnessLocation = Raylib.GetShaderLocation(shader, SMOOTHNESS_NAME);
        RoundnessLocation  = Raylib.GetShaderLocation(shader, ROUNDNESS_NAME);
    }

    public override void Unload()
    {
        Assets.Shaders.Unload(SHADER_ASSET_PATH);
    }

    public override void Defaults()
    {
        SetColor(Color.Black);
        SetCenter(0.5f, 0.5f);
        SetIntensity(0.4f);
        SetSmoothness(0.2f);
        SetRoundness(1f);
    }

    protected override ref Shader GetShader() => ref shader;
}