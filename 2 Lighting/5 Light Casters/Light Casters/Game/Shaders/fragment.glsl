#version luma-dx

uniform vec3 objectColour;

uniform lx_SpecTexMaterial material;
uniform lx_Light light;

// view space vectors (where camera position is the origin)
in vec3 vNormal;
in vec3 vFragPos;
in vec3 vLightPos;

in vec2 texCoords;

[shader]

void main()
{
    lx_FragColour = vec4(lx_vPhong(vNormal,vFragPos,vLightPos,texCoords,material,light),1.0);
}


[lightShader]

void main()
{
    lx_FragColour = vec4(light.ambient,1.0);
}