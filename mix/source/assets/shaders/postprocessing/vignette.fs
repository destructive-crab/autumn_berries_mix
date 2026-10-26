#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;

uniform vec3  color;   // Unity "Color"
uniform vec2  center;          // Unity "Center", normalized (0.5,0.5 = screen center)
uniform float intensity;       // Unity "Intensity", 0..1
uniform float smoothness;      // Unity "Smoothness", > 0 (e.g. 0.01..1)
uniform float roundness;       // Unity "Roundness", 0 = square, 1 = circular

out vec4 finalColor;

void main()
{
    vec4 texel = texture(texture0, fragTexCoord);

    // Convert UV to screen-space pixel coordinates.
    // fragTexCoord.y is flipped for raylib render textures.
    vec2 texSize = vec2(textureSize(texture0, 0));
    vec2 pixel = vec2(fragTexCoord.x, 1.0 - fragTexCoord.y) * texSize;

    // Center in pixels, derived inside the shader from normalized input
    vec2 centerPx = center * texSize;

    // Offset from center, in pixels
    vec2 d = pixel - centerPx;

    // Unity-style distance: circular vs square blended by roundness
    float distCircle = length(d);
    float distSquare = max(abs(d.x), abs(d.y));
    float dist = mix(distSquare, distCircle, clamp(roundness, 0.0, 1.0));

    // Normalize by the half-diagonal so intensity behaves like Unity's 0..1 slider
    float halfDiagonal = length(texSize * 0.5);
    float normDist = dist / halfDiagonal;

    // Unity's falloff: 1 at center, 0 at corners, shaped by intensity and smoothness.
    // This replaces the separate "radius" concept.
    float inner = 1.0 - clamp(intensity, 0.0, 1.0);
    float outer = inner + max(smoothness, 0.0001);

    float mask = smoothstep(inner, outer, normDist);

    vec3 result = mix(texel.rgb, color, mask);

    finalColor = vec4(result, texel.a) * colDiffuse * fragColor;
}