#version luma-dx

uniform lx_Light light;
uniform lx_Material material;
uniform samplerCube cubemap;

uniform vec3 cameraPos;

in vec2 texCoords;
in vec3 normal;
in vec3 textureDir;
in vec3 fragPos;

[scene]
void main()
{
    
    lx_FragColour = vec4(lx_Phong(normal, fragPos, cameraPos, texCoords, material, light),1.0);//texture(texture0,texCoords);
}

[cubemap]
void main()
{
    lx_FragColour = 0.8*texture(cubemap,textureDir);
}