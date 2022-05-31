#version 330 core
layout (location = 0) in vec3 aPos;

out vec4 vertexColour;

uniform vec3 inputColour2;

void main()
{
    vertexColour = vec4(inputColour2,1.0);
    gl_Position = vec4(aPos, 1.0);
}