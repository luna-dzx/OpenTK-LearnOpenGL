#version luma-dx

uniform sampler2D texture0;
uniform samplerCube cubemap;

in vec3 fragPos;
in vec3 normal;
in vec3 textureDir;

uniform vec3 cameraPos;

[scene]
void main()
{
    float ratio = 1.00 / 1.33;
    vec3 viewDir = normalize(fragPos - cameraPos);
    vec3 refraction = refract(viewDir,normalize(normal), ratio);
    lx_FragColour = texture(cubemap,refraction);
}

[cubemap]
void main()
{
    lx_FragColour = texture(cubemap,textureDir);
}