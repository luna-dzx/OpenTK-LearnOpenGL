#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColour;
layout (location = 2) in vec2 aTexCoord;

out vec3 colour;
out vec2 texCoord;

void main()
{
    texCoord = aTexCoord;
    colour = aColour;
    gl_Position = vec4(aPos, 1.0);
}