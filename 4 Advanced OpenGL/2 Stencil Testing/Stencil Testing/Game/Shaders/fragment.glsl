#version luma-dx

uniform sampler2D floorTexture;
uniform sampler2D cubeTexture;

in vec2 texCoords;


[floor]

void main()
{
    lx_FragColour = texture(floorTexture,texCoords);
}


[cube]

void main()
{
    lx_FragColour = texture(cubeTexture,texCoords);
}

[flatColour]

void main()
{
    lx_FragColour = vec4(0.04, 0.28, 0.26, 1.0);
}