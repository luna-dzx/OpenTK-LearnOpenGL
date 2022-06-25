#version luma-dx
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;

uniform lx_Light light;

out vec3 vNormal;
out vec3 vFragPos;
out vec3 vLightPos;
out lx_Light testlight;
out vec2 texCoords;

void main()
{
    texCoords = aTexCoords;

    // apply normal matrix to fix non uniform scaling
    vNormal = mat3(transpose(inverse(lx_View*lx_Model))) * aNormal;
    vLightPos = (lx_View*vec4(light.position,1.0)).xyz;
    vFragPos = (lx_View*lx_Model*vec4(aPos,1.0)).xyz;
    
    gl_Position = lx_Transform*vec4(aPos, 1.0);
}