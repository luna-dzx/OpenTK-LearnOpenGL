#version luma-dx

uniform lx_Material material;
uniform lx_Light light;
uniform vec3 cameraPos;

uniform sampler2D depthMap;

in vec2 texCoords;
in vec3 normal;
in vec3 fragPos;

[depthMap]
void main()
{

}

[scene]
void main()
{
    lx_FragColour = vec4(lx_Phong(normal, fragPos, cameraPos, texCoords, material, light),1.0);
}

[test]
void main()
{
    lx_FragColour = vec4(texture(depthMap,texCoords).r);
}