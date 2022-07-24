#version luma-dx

uniform sampler2D texture0;

in vec2 texCoords;

[scene]

void main()
{
    lx_FragColour = texture(texture0,texCoords);
}

[quad]
void main()
{
    lx_FragColour = texture(texture0,texCoords);
}