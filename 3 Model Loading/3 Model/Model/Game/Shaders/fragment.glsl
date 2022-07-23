#version luma-dx

uniform lx_Light light;
uniform lx_Material material;

uniform vec3 cameraPos;

in vec2 texCoords;
in vec3 normal;
in vec3 fragPos;

[main]

void main()
{
    lx_FragColour = vec4(lx_Phong(normal, fragPos, cameraPos, texCoords, texCoords, material, light),1.0);
}


[light]

void main()
{
    lx_FragColour = vec4(1.0);
}