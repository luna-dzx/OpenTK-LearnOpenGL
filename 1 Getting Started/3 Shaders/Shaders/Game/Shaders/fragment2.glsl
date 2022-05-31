#version 330 core

out vec4 FragColor;

in vec4 vertexColour;

void main()
{
   FragColor = vertexColour;
}