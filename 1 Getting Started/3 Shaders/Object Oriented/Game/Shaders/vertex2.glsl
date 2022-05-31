#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColour;

out vec4 vertexColour;

uniform float triangleXpos;

void main()
{
    vertexColour = vec4(aColour,1.0);
    gl_Position = vec4(triangleXpos + aPos.x,-aPos.y,aPos.z, 1.0);
}