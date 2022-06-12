#version 330 core

uniform vec3 colour;

[shader]

void main()
{
    lx_FragColour = vec4(colour,1.0);
}


[lightShader]

void main()
{
    lx_FragColour = vec4(1.0);
}