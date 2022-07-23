#version luma-dx

uniform sampler2D floorTexture;
uniform sampler2D cubeTexture;

uniform int visualDepth;

in vec2 texCoords;

float near = 0.1;
float far = 100.0;

float LinearizeDepth(float depth) 
{
    float z = depth * 2.0 - 1.0; // back to NDC 
    return (2.0 * near * far) / (far + near - z * (far - near));	
}


vec4 ColourCalculation(sampler2D tex)
{
    if (visualDepth == 0){
        return texture(tex,texCoords);
    }
    if (visualDepth == 1){
        return vec4(gl_FragCoord.z);
    }
    if (visualDepth == 2){
        return vec4(LinearizeDepth(gl_FragCoord.z) / far);
    }
}


[floor]

void main()
{
    lx_FragColour = ColourCalculation(floorTexture);
}


[cube]

void main()
{
    lx_FragColour = ColourCalculation(cubeTexture);
}