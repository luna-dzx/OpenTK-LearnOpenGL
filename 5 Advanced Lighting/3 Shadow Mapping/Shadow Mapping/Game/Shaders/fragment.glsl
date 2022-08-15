﻿#version luma-dx

uniform lx_Material material;
uniform lx_Light light;
uniform vec3 cameraPos;

uniform sampler2D depthMap;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
    vec4 fragPosLightSpace;
} fs_in;

float ShadowCalculation(vec4 fragPosLightSpace, float bias)
{
    vec3 projCoords = (fragPosLightSpace.xyz / fragPosLightSpace.w) * 0.5 + 0.5;
    float closestDepth = texture(depthMap, projCoords.xy).r;
    float currentDepth = projCoords.z;
    
    float shadow = (currentDepth - bias > texture(depthMap, projCoords.xy).r ? 1.0 : 0.0);
    /*if (shadow == 0.0)
    {
        vec2 texelSize = 1.0 / textureSize(depthMap, 0);
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                float pcfDepth = texture(depthMap, projCoords.xy + vec2(x, y) * texelSize).r; 
                shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;        
            }    
        }
        shadow /= 4.5;
        
    }*/

    
    if(projCoords.z > 1.0)
        shadow = 0.0;
        
        
    return 1.0-shadow;
}


[depthMap]
void main()
{

}

[scene]
void main()
{
    float bias = 0.0;//max(0.01 * (1.0 - abs(dot(fs_in.normal, light.direction))), 0.005);
    lx_FragColour = vec4(lx_Phong(fs_in.normal, fs_in.fragPos, cameraPos, fs_in.texCoords, material, light, ShadowCalculation(fs_in.fragPosLightSpace,bias)),1.0);
}