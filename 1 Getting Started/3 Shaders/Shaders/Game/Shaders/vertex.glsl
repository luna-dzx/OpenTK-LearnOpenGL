#version 330 core
layout (location = 0) in vec3 aPos;

out vec4 vertexColour;

uniform vec4 

void main()
{
    vertexColour = vec4(1.0,1.0,0.0,1.0);
    gl_Position = vec4(aPos, 1.0);
}