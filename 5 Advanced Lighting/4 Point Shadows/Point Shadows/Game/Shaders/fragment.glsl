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


float ShadowCalculation(samplerCube depthMap, vec3 fragPos, vec3 lightPos, float farPlane)
{
    vec3 fragToLight = fragPos - lightPos;
    float closestDepth = texture(depthMap, fragToLight).r;
    closestDepth *= farPlane;
    float currentDepth = length(fragToLight);
    
    float bias = 0.05;
    float shadow = currentDepth - bias > closestDepth ? 0.0 : 1.0;
    
    return shadow;
}



[cube]
void main()
{

    
    float shadow = ShadowCalculation(depthMap, fs_in.fragPos, light.position, farPlane);//lx_ShadowCalculation(depthMap,fs_in.fragPosLightSpace,3.0,2,10.0);
    lx_FragColour = vec4(lx_Phong(fs_in.normal, fs_in.fragPos, cameraPos, fs_in.texCoords, material, light, shadow),1.0);
    //lx_FragColour = vec4(texture(depthMap, cameraPos).r);
    //lx_FragColour = vec4(gl_FragDepth);
  
    
    
    
}

[light]
void main()
{
    lx_FragColour = vec4(1.0);
}