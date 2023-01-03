#version luma-dx

uniform lx_Material material;
uniform lx_Light light;
uniform vec3 cameraPos;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
} fs_in;

[scene]
void main()
{
    vec3 phong = lx_Phong(fs_in.normal, fs_in.fragPos, cameraPos, fs_in.texCoords, material, light, 1.0);
    lx_FragColour = lx_Colour(phong);
}

[light]
void main()
{
    lx_FragColour = vec4(1.0);
}