#version luma-dx

uniform vec3 objectColour;
uniform vec3 lightColour;

uniform lx_Material material;
uniform lx_Light light;

uniform sampler2D boxTexture;

// view space vectors (where camera position is the origin)
in vec3 vNormal;
in vec3 vFragPos;
in vec3 vLightPos;

in vec2 texCoords;

[shader]

void main()
{
    lx_Light vLight = light;
    vLight.position = vLightPos;
    lx_FragColour = texture(boxTexture,texCoords) * vec4(lx_vPhong(vNormal,vFragPos,material,vLight),1.0);
}


[lightShader]

void main()
{
    lx_FragColour = vec4(lightColour,1.0);
}