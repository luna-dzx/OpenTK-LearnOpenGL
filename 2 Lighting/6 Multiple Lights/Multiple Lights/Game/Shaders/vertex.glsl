#version luma-dx
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;


out vec3 normal;
out vec3 fragPos;
out vec2 texCoords;

void main()
{
    texCoords = aTexCoords;

    // apply normal matrix to fix non uniform scaling
    normal = lx_NormalFix(lx_Model, aNormal);
    //vLightPos = (lx_View*vec4(light.position,1.0)).xyz;
    fragPos = (lx_Model*vec4(aPos,1.0)).xyz;
    
    gl_Position = lx_Transform*vec4(aPos, 1.0);
}