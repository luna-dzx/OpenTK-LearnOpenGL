#version luma-dx

uniform sampler2D fboTexture;

in vec2 texCoords;

[scene]
void main()
{ 
    lx_FragColour = vec4(0.0,1.0,0.0,1.0);
}

[fbo]
void main()
{ 
    lx_FragColour = texture(fboTexture,texCoords);
}
