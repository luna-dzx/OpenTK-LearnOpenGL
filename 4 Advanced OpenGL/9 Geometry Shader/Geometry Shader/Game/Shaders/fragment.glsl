#version luma-dx

uniform sampler2D texture0;

in vec2 texCoords;
in vec3 normal;


[main]
void main()
{
    lx_FragColour = texture(texture0,texCoords);
}

[alt]
void main()
{
    lx_FragColour = vec4(normal,1.0);
}