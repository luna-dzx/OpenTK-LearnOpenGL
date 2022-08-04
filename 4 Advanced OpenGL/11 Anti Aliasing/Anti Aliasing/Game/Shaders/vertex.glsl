#version luma-dx
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
//layout (location = 2) in vec3 aNormal;

out vec2 texCoords;

[scene]
void main()
{
    gl_Position = lx_Transform*vec4(aPos, 1.0);
}

[fbo]
void main()
{
    texCoords = aTexCoords;
    gl_Position = vec4(aPos, 1.0);
}
