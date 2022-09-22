#version 330 core

uniform sampler2D sampler;
in vec2 texCoords;

out vec4 fragColour;

void main()
{
    fragColour = texture(sampler,texCoords);
}