#version luma-dx
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
//layout (location = 2) in vec3 aNormal;

out VS_OUT
{
    vec2 texCoords;
} vs_out;

[points]
void main()
{
    gl_Position = lx_Transform*vec4(aPos, 1.0);
    gl_PointSize = 30.0/gl_Position.z;
}

[main]
void main()
{
    vs_out.texCoords = aTexCoords;
    gl_Position = lx_Transform*vec4(aPos, 1.0);
}