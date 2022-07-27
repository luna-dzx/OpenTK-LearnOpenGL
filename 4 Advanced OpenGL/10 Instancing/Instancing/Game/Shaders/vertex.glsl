#version luma-dx
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in mat4 instanceMatrix;


out vec2 texCoords;
out vec3 normal;
out vec3 textureDir;
out vec3 fragPos;


[asteroid]
void main()
{
    texCoords = aTexCoords;
    normal = lx_NormalFix(instanceMatrix,aNormal);//(instanceMatrix*vec4(aNormal,1.0)).xyz;
    fragPos = aPos;
    gl_Position = lx_Transform*instanceMatrix*vec4(aPos, 1.0);
}

[planet]
void main()
{
    texCoords = aTexCoords;
    normal = lx_NormalFix(lx_Model,aNormal);
    fragPos = aPos;
    gl_Position = lx_Transform*vec4(aPos, 1.0);
}

[cubemap]
void main()
{
    textureDir = aPos;
    mat4 view = mat4(mat3(lx_View));
    gl_Position = (lx_Proj * view *vec4(aPos, 1.0));
}
