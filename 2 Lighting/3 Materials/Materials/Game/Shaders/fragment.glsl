#version luma-dx

uniform vec3 objectColour;
uniform vec3 lightColour;

// view space vectors (where camera position is the origin)
in vec3 vNormal;
in vec3 vFragPos;
in vec3 vLightPos;

[shader]

void main()
{
    vec3 result = (0.1+lx_ViewPhong(vFragPos,vLightPos,vNormal,0.5,32)) *lightColour *objectColour;
    lx_FragColour = vec4(result,1.0);
}


[lightShader]

void main()
{
    lx_FragColour = vec4(lightColour,1.0);
}