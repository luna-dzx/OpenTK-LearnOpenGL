#version 330 core

layout (location = 0) out vec4 fragColour;
layout (location = 1) out vec4 brightColour;

uniform sampler2D sampler;
uniform sampler2D brightSample;
in vec2 texCoords;

void main()
{
    fragColour = texture(sampler,texCoords);
    brightColour = texture(brightSample,texCoords);
}