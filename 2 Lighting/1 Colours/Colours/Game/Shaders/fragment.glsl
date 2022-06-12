#version 330 core
out vec4 pixelColour;

uniform vec3 colour;

void main()
{
   pixelColour = vec4(colour,1.0);
}