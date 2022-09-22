#version luma-dx

uniform sampler2D sampler;
in vec2 texCoords;

uniform float exposure;
uniform int highDynamicRange;

void main()
{
    vec3 colour = texture(sampler, texCoords).rgb;

    if (highDynamicRange == 0) { lx_FragColour = vec4(colour,1.0); }
    else { lx_FragColour = vec4(lx_ApplyHDR(colour,exposure,2.2), 1.0); }
    
    // here we can freely use vec4(...,1.0); since this is a framebuffer
    // over the whole screen which will never use transparency / alpha

}