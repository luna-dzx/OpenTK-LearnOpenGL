#version luma-dx

uniform sampler2D floorTexture;
uniform sampler2D grassTexture;
uniform sampler2D glassTexture;

uniform vec3 filterColour;

in vec2 texCoords;


[floor]

void main()
{
    lx_FragColour = texture(floorTexture,texCoords);
}


[grass]

void main()
{
    vec4 colour = texture(grassTexture,texCoords);
    
    if (colour.a < 0.1) discard;
    
    lx_FragColour = colour;
}

[glass]

void main()
{
    vec4 colour = texture(glassTexture,texCoords);
    
    if (colour.r > 0.8)
    {
        colour = vec4(filterColour,colour.a);
    }
    
    lx_FragColour = colour;
}
