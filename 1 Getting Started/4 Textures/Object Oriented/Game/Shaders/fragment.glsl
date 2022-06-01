#version 330 core
out vec4 pixelColour;

in vec3 colour;
in vec2 texCoord;

uniform sampler2D texture0;
uniform sampler2D texture1;

uniform float mixValue;

void main()
{
   pixelColour = mix(texture(texture0, texCoord), texture(texture1, texCoord*2.0), mixValue) * vec4(colour,1.0);
}