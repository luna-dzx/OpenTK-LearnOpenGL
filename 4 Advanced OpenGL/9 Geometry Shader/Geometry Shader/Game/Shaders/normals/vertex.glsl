#version luma-dx
layout (location = 0) in vec3 aPos;
//layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;

out VS_OUT {
    vec3 normal;
} vs_out;

void main()
{
    vs_out.normal = lx_NormalFix(lx_View*lx_Model,aNormal);
    gl_Position = lx_View*lx_Model*vec4(aPos, 1.0);
}