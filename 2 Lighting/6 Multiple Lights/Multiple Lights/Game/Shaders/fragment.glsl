#version luma-dx

uniform lx_Material material;

#define NUM_LIGHTS 3
uniform lx_Light lights[NUM_LIGHTS];
uniform vec3 cameraPos;

in vec3 normal;
in vec3 fragPos;

in vec2 texCoords;

[shader]

void main()
{

    vec3 lightColour = vec3(0.0);

    for (int i = 0; i < NUM_LIGHTS; i++)
    {
        lightColour+=lx_Phong(normal,fragPos,cameraPos,texCoords,texCoords,material,lights[i]);
    }

    lx_FragColour = vec4(lightColour,1.0);
}


[lightShader]

void main()
{
    lx_FragColour = vec4(1.0);
}