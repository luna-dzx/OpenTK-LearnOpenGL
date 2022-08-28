#version luma-dx

uniform lx_Material material;
uniform lx_Light light;
uniform vec3 cameraPos;

uniform samplerCube depthMap;
uniform int visualiseDepthMap;

uniform float farPlane;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
} fs_in;

vec3 sampleOffsetDirections[20] = vec3[]
(
   vec3( 1,  1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1,  1,  1), 
   vec3( 1,  1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1,  1, -1),
   vec3( 1,  1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1,  1,  0),
   vec3( 1,  0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1,  0, -1),
   vec3( 0,  1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0,  1, -1)
);   

float ShadowCalculation(samplerCube depthMap, vec3 fragPos, vec3 lightPos, vec3 cameraPos, float farPlane)
{
    vec3 fragToLight = fragPos - lightPos;
    float closestDepth = texture(depthMap, fragToLight).r;
    closestDepth *= farPlane;
    float currentDepth = length(fragToLight);
    
    float bias = 0.15;
    float shadow = currentDepth - bias > closestDepth ? 0.0 : 1.0;
    
    
    if (shadow == 0.0)
    {
        int samples = 20;
        float viewDistance = length(cameraPos - fragPos);
        float diskRadius = (1.0 + (viewDistance / farPlane)) / 30.0; 
        for(int i = 0; i < samples; ++i)
        {
            closestDepth = texture(depthMap, fragToLight + sampleOffsetDirections[i] * diskRadius).r;
            closestDepth *= farPlane;
            if(currentDepth - bias < closestDepth)
                shadow += 1.0;
        }
        shadow /= float(samples);
    
    }

    
    return shadow;
}



[cube]
void main()
{

    
    float shadow = ShadowCalculation(depthMap, fs_in.fragPos, light.position, cameraPos, farPlane);//lx_ShadowCalculation(depthMap,fs_in.fragPosLightSpace,3.0,2,10.0);
    lx_FragColour = vec4(lx_Phong(fs_in.normal, fs_in.fragPos, cameraPos, fs_in.texCoords, material, light, shadow),1.0);
    //lx_FragColour = vec4(texture(depthMap, cameraPos).r);
    //lx_FragColour = vec4(gl_FragDepth);
  
    
    
    
}

[light]
void main()
{
    lx_FragColour = vec4(1.0);
}