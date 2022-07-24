#version luma-dx

uniform sampler2D texture0;

in vec2 texCoords;

const float offset = 1.0 / 300.0;  

[scene]

void main()
{
    lx_FragColour = texture(texture0,texCoords);
}

[quad]
void main()
{
    //vec4 colour = texture(texture0,texCoords);
    
    // invert colours
    //lx_FragColour = vec4( vec3(1.0 - colour), 1.0); 
    
    // greyscale
    //float average = 0.2126 * colour.r + 0.7152 * colour.g + 0.0722 * colour.b;
    //lx_FragColour = vec4(average,average,average,1.0);

    // kernel

    float kernel[9] = float[](
        -1, -1, -1,
        -1,  9, -1,
        -1, -1, -1
    );
    
    vec3 colour = vec3(0.0);
    
    for(int x = -1; x <= 1; x++)
    {
        for(int y = -1; y <= 1; y++)
        {
            int index = (x+1) + (y+1)*3;
            vec2 pixelOffset = vec2(float(x)*offset, float(y)*offset);
            colour += (texture(texture0,texCoords + pixelOffset)).xyz * kernel[index];
        }
    }
    
    lx_FragColour = vec4(colour,1.0);
    
    
}