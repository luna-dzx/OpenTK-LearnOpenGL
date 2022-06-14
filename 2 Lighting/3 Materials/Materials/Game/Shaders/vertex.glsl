#version luma-dx
layout (location = 0) in vec3 aPos;
// location 1 reserved for texture coordinates
layout (location = 2) in vec3 aNormal;

uniform lx_Light light;

out vec3 vNormal;
out vec3 vFragPos;
out vec3 vLightPos;

void main()
{
    // apply normal matrix to fix non uniform scaling
    vNormal = mat3(transpose(inverse(lx_View*lx_Model))) * aNormal;
    vLightPos = (lx_View*vec4(light.position,1.0)).xyz;
    vFragPos = (lx_View*lx_Model*vec4(aPos,1.0)).xyz;
    
    gl_Position = lx_Transform*vec4(aPos, 1.0);
}