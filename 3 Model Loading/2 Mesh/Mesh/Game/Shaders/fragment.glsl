#version luma-dx

uniform sampler2D texture0;

in vec2 texCoords;

void main()
{
    lx_FragColour = texture(texture0,texCoords);
}