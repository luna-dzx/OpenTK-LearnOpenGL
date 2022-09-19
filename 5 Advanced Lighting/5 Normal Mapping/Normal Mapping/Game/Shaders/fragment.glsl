#version luma-dx

uniform lx_Material material;
uniform lx_Light light;
uniform sampler2D normalMap;
uniform vec3 cameraPos;

uniform int normalMapping;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
    vec3 TBNfragPos;
    vec3 TBNlightPos;
    vec3 TBNcameraPos;
    vec3 tangent;
} fs_in;

[scene]
void main()
{
    if (normalMapping == 0)
    {
        lx_FragColour = vec4(lx_Phong(lx_NormalFlipVec(cameraPos, fs_in.normal), fs_in.fragPos, cameraPos, fs_in.texCoords, material, light, 1.0),1.0);
    }
    else
    {
        vec3 normal = lx_NormalMap(normalMap,fs_in.texCoords) * lx_NormalFlip(cameraPos, fs_in.normal);
        lx_FragColour = lx_Colour(lx_Phong(normal, fs_in.TBNfragPos, fs_in.TBNcameraPos, fs_in.texCoords, material, lx_MoveLight(light,fs_in.TBNlightPos), 1.0));
    }

}

[light]
void main()
{
    lx_FragColour = vec4(1.0);
}