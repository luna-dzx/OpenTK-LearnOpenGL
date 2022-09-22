#version luma-dx

uniform sampler2D sampler;
uniform sampler2D brightSample;
in vec2 texCoords;

uniform float exposure;
uniform int highDynamicRange;

uniform int blurDirection; // 0 for horizontal, 1 for vertical

uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

[hdr]
void main()
{
    vec3 colour = texture(sampler, texCoords).rgb;
    
    vec3 bloomColour = texture(brightSample, texCoords).rgb;
    
    lx_FragColour = vec4(lx_ApplyHDR(colour+bloomColour,exposure,2.2), 1.0);
    
    // here we can freely use vec4(...,1.0); since this is a framebuffer
    // over the whole screen which will never use transparency / alpha

}

[gaussian]
void main()
{
    
    vec2 tex_offset = 1.0 / textureSize(brightSample, 0); // gets size of single texel
    vec3 colour = texture(brightSample, texCoords).rgb * weight[0]; // current fragment's contribution
    
    if(blurDirection==0) // horizontal
    {
        for(int i = 1; i < 5; ++i)
        {
            colour += texture(brightSample, texCoords + vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
            colour += texture(brightSample, texCoords - vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
        }
    }
    else // vertical
    {
        for(int i = 1; i < 5; ++i)
        {
            colour += texture(brightSample, texCoords + vec2(0.0, tex_offset.y * i)).rgb * weight[i];
            colour += texture(brightSample, texCoords - vec2(0.0, tex_offset.y * i)).rgb * weight[i];
        }
    }
    
    lx_FragColour = vec4(colour, 1.0);

}