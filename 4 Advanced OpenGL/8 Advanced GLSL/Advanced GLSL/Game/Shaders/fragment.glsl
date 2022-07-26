#version luma-dx

uniform sampler2D texture0;

in VS_OUT
{
    vec2 texCoords;
} fs_in;

[points]
void main()
{
    lx_FragColour = vec4(0.8);
}

[main]
void main()
{
    if (gl_FrontFacing)
    {
        if (gl_FragCoord.x < 400)
            lx_FragColour = vec4(0.99,0.31,0.36,1.0);
        else
            lx_FragColour = vec4(0.36, 0.31, 0.96,1.0);
    }
    else
    {
        lx_FragColour = texture(texture0,fs_in.texCoords);
    }
    
    //gl_FragDepth = gl_FragCoord.z + 0.05;
}
