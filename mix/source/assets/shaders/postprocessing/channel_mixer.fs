#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec4 colDiffuse;

out vec4 finalColor;

uniform vec3 outRed;    
uniform vec3 outGreen; 
uniform vec3 outBlue; 

void main()
{
    vec4 texel = texture(texture0, fragTexCoord);
    vec3 color = texel.rgb;

    vec3 mixed = vec3(
        dot(color, outRed),
        dot(color, outGreen),
        dot(color, outBlue)
    );

    mixed = clamp(mixed, -2.0, 2.0);

    finalColor = vec4(mixed, texel.a) * colDiffuse * fragColor;
}