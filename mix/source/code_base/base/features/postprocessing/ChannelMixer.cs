using Raylib_cs;

namespace mix.gameplay.systems;

public sealed class ChannelMixer : PostprocessingOverride
{
    private Shader shader;

    private const string SHADER_ASSET_PATH = "assets/shaders/postprocessing/channel_mixer.fs";

    private const string OUT_RED_NAME = "outRed";
    private const string OUT_GREEN_NAME = "outGreen";
    private const string OUT_BLUE_NAME = "outBlue";
    
    private int OutRedLocation;
    private int OutGreenLocation;
    private int OutBlueLocation;

    public ChannelMixer SetRed  (float r, float g, float b) { SetProperty(OutRedLocation,   r, g, b); return this; }
    public ChannelMixer SetGreen(float r, float g, float b) { SetProperty(OutGreenLocation, r, g, b); return this; }
    public ChannelMixer SetBlue (float r, float g, float b) { SetProperty(OutBlueLocation,  r, g, b); return this; }

    private void SetProperty(int location, float r, float g, float b)
    {
        Raylib.SetShaderValue(shader, location, new float[] { r, g, b }, ShaderUniformDataType.Vec3);
    }
    
    public override void Load()
    {
        shader = Assets.Shaders.Load(SHADER_ASSET_PATH);

        OutRedLocation   = Raylib.GetShaderLocation(shader, OUT_RED_NAME);
        OutGreenLocation = Raylib.GetShaderLocation(shader, OUT_GREEN_NAME);
        OutBlueLocation  = Raylib.GetShaderLocation(shader, OUT_BLUE_NAME);
    }

    public override void Unload()
    {
        Assets.Shaders.Unload(SHADER_ASSET_PATH);
    }

    public override void Defaults()
    {
        SetRed  (1, 0, 0);
        SetGreen(0, 1, 0);
        SetBlue (0, 0, 1);
    }

    protected override ref Shader GetShader() => ref shader;
}