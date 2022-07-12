#version luma-dx

uniform vec3 objectColour;

uniform lx_Material material;
uniform lx_Light light;
uniform vec3 cameraPos;

in vec3 normal;
in vec3 fragPos;

in vec2 texCoords;

[shader]

void main()
{
    lx_FragColour = vec4(lx_Phong(normal,fragPos,cameraPos,texCoords,texCoords,material,light),1.0);
}


[lightShader]

void main()
{
    lx_FragColour = vec4(light.ambient,1.0);
}