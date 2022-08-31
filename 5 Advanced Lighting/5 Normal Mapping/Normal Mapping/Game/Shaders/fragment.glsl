#version luma-dx

uniform lx_Material material;
uniform lx_Light light;
uniform vec3 cameraPos;
uniform sampler2D normalMap;

uniform int normalMapping;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
} fs_in;

void main()
{

    
    vec3 normal;
    if (normalMapping == 0)
    {
        normal = fs_in.normal;
    }
    else
    {
        normal = normalize(texture(normalMap, fs_in.texCoords).rgb * 2.0 - 1.0);
    }
    
    lx_FragColour = vec4(lx_Phong(normal, fs_in.fragPos, cameraPos, fs_in.texCoords, material, light, 1.0),1.0);
}