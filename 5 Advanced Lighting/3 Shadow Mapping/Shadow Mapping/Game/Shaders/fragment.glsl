#version luma-dx

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

float ShadowCalculation(vec4 fragPosLightSpace)
{
    vec3 projCoords = (fragPosLightSpace.xyz / fragPosLightSpace.w) * 0.5 + 0.5;
    float closestDepth = texture(depthMap, projCoords.xy).r;
    float currentDepth = projCoords.z;
    float textureDepth = texture(depthMap, projCoords.xy).r;
    
    float shadow = (currentDepth > textureDepth ? 1.0 : 0.0);
    

    if (shadow == 0.0)
    {
        vec2 texelSize = 3.0 / textureSize(depthMap, 0);
        for(int x = -2; x <= 2; x+=1)
        {
            for(int y = -2; y <= 2; y+=1)
            {
                float pcfDepth = texture(depthMap, projCoords.xy + vec2(x, y) * texelSize).r; 
                shadow += currentDepth > pcfDepth ? 1.0 : 0.0;
            }    
        }
        shadow = min(shadow/10.0,0.98);
    }
    
    if(projCoords.z > 1.0)
        shadow = 0.0;
        
        
    return 1.0-shadow;
}

void main()
{
    if (visualiseDepthMap == 0){
        lx_FragColour = vec4(lx_Phong(fs_in.normal, fs_in.fragPos, cameraPos, fs_in.texCoords, material, light, ShadowCalculation(fs_in.fragPosLightSpace)),1.0);
    } else {
        vec2 projCoords = (fs_in.fragPosLightSpace.xy / fs_in.fragPosLightSpace.w) * 0.5 + 0.5;
        lx_FragColour = vec4(texture(depthMap,projCoords).r);
    }
    
}