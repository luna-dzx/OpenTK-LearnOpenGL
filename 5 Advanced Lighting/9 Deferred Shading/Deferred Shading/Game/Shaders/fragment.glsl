#version luma-dx

uniform lx_Material material;
uniform lx_Light light;
uniform sampler2D normalMap;
uniform vec3 cameraPos;

layout (location = 0) out vec4 fragColour;
layout (location = 1) out vec4 brightColour;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
    vec3 TBNfragPos;
    vec3 TBNlightPos;
    vec3 TBNcameraPos;
    vec3 tangent;
} fs_in;


vec3 CalculateBrightColour(vec3 colour)
{
    float brightness = dot(colour, vec3(0.2126, 0.7152, 0.0722));
    if(brightness > 1.0) { return colour; }
    return vec3(0.0);
}

[scene]
void main()
{
    vec3 normal = lx_NormalMap(normalMap,fs_in.texCoords);
    vec3 phong = lx_Phong(normal, fs_in.TBNfragPos, fs_in.TBNcameraPos, fs_in.texCoords, fs_in.texCoords, material, lx_MoveLight(light,fs_in.TBNlightPos), 1.0);
    fragColour = lx_Colour(phong);
    brightColour = lx_Colour(CalculateBrightColour(fragColour.rgb));
}

[light]
void main()
{
    fragColour = vec4(12.0);
    brightColour = vec4(12.0);
}