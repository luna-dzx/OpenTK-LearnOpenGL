#version 330 core

out vec4 pixelColour;

in vec4 vertexColour;

void main()
{
   pixelColour = vertexColour;
}