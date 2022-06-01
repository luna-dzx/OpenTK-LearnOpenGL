#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColour;
layout (location = 2) in vec2 aTexCoord;

out vec3 colour;
out vec2 texCoord;

uniform mat4 transform;

void main()
{
    texCoord = aTexCoord;
    colour = aColour;
    gl_Position = transform*vec4(aPos, 1.0);
}