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
    lx_FragColour = vec4(fs_in.tangent,1.0);
    
    if (normalMapping == 0)
    {
        lx_FragColour = vec4(lx_Phong(fs_in.normal, fs_in.fragPos, cameraPos, fs_in.texCoords, material, light, 1.0),1.0);
    }
    else
    {
        vec3 normal = normalize(texture(normalMap, fs_in.texCoords).rgb * 2.0 - 1.0);
        float mult = lx_Diffuse(fs_in.normal,fs_in.fragPos,light.position);
        lx_Light adjLight = light;
        adjLight.position = fs_in.TBNlightPos;
        lx_FragColour = vec4(lx_Phong(normal, fs_in.TBNfragPos, fs_in.TBNcameraPos, fs_in.texCoords, material, adjLight, 1.0),1.0);
    }
    

}

[light]
void main()
{
    lx_FragColour = vec4(1.0);
}