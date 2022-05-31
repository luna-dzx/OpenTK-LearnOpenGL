#version 330 core
out vec4 pixelColour;

in vec3 colour;
in vec2 texCoord;

uniform sampler2D texture0;

void main()
{
   pixelColour = texture(texture0,texCoord) * vec4(colour,1.0);
}