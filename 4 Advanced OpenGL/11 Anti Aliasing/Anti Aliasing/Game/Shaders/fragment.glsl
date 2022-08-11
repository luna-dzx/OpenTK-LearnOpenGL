#version luma-dx

uniform sampler2DMS fboTexture;

in vec2 texCoords;

uniform vec3 cubeColour;

[scene]
void main()
{ 
    lx_FragColour = vec4(cubeColour,1.0);
}

[fbo]
void main()
{
    ivec2 pixelCoords = ivec2(int(800.0*texCoords.x),int(600.0*texCoords.y));
    if (pixelCoords.x < 500){
        lx_FragColour = lx_MultiSample(fboTexture, pixelCoords, 4);
        //lx_FragColour = texture(fboTexture,texCoords);
    }else{
        lx_FragColour = vec4(vec3(texCoords,0.0),0.0);
    }
    
    //lx_FragColour = texture(fboTexture,texCoords);
}
