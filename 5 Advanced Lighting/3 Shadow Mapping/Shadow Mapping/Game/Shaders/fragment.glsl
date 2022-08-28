﻿#version luma-dx

uniform lx_Material material;
uniform lx_Light light;
uniform vec3 cameraPos;

uniform sampler2D depthMap;
uniform int visualiseDepthMap;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
    vec4 fragPosLightSpace;
} fs_in;

void main()
{
    if (visualiseDepthMap == 0){
        float shadow = lx_ShadowCalculation(depthMap,fs_in.fragPosLightSpace,3.0,2,10.0);
        lx_FragColour = vec4(lx_Phong(fs_in.normal, fs_in.fragPos, cameraPos, fs_in.texCoords, material, light, shadow),1.0);
    } else {
        vec2 projCoords = (fs_in.fragPosLightSpace.xy / fs_in.fragPosLightSpace.w) * 0.5 + 0.5;
        lx_FragColour = vec4(texture(depthMap,projCoords).r);
    }
    
}