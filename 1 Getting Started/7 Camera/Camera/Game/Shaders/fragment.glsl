#version 330 core
out vec4 pixelColour;

in vec2 texCoord;

uniform sampler2D texture0;
uniform sampler2D texture1;

uniform float mixValue;

void main()
{
   pixelColour = mix(texture(texture0, texCoord), texture(texture1, texCoord), mixValue);
}