#version luma-dx

uniform lx_Material material;
uniform lx_Light light;
uniform vec3 cameraPos;

in vec2 texCoords;
in vec3 normal;
in vec3 fragPos;

[scene]
void main()
{
    lx_FragColour = vec4(lx_Phong(normal, fragPos, cameraPos, texCoords, material, light),1.0);
}

[light]
void main()
{
    lx_FragColour = vec4(1.0);
}
