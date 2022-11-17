#version luma-dx

uniform lx_Material material;
uniform lx_Material cubeMaterial;
uniform lx_Light light;
uniform vec3 cameraPos;

uniform samplerCube depthMap;
uniform int visualiseDepthMap;

uniform sampler2D normalMap;
uniform sampler2D displaceMap;

uniform float farPlane;
uniform float height_scale;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
    vec3 TBNfragPos;
    vec3 TBNlightPos;
    vec3 TBNcameraPos;
} fs_in;


[cube]
void main()
{
    float shadow = lx_ShadowCalculation(depthMap, fs_in.fragPos, light.position, cameraPos, farPlane);//lx_ShadowCalculation(depthMap,fs_in.fragPosLightSpace,3.0,2,10.0);
    lx_FragColour = lx_Colour(lx_Phong(fs_in.normal, fs_in.fragPos, cameraPos, fs_in.texCoords, fs_in.texCoords, cubeMaterial, light, shadow));
}

[brick]
void main()
{
    vec2 texCoords = lx_ParallaxMapping(displaceMap, fs_in.texCoords,  normalize(fs_in.TBNcameraPos - fs_in.TBNfragPos), height_scale, 8.0, 64.0);
    if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0) discard;
    
    vec3 normal = lx_NormalMap(normalMap,texCoords);
    
    float shadow = lx_ShadowCalculation(depthMap, fs_in.fragPos, light.position, cameraPos, farPlane);//lx_ShadowCalculation(depthMap,fs_in.fragPosLightSpace,3.0,2,10.0);
    lx_FragColour = lx_Colour(lx_Phong(normal, fs_in.TBNfragPos, fs_in.TBNcameraPos, texCoords, material, lx_MoveLight(light,fs_in.TBNlightPos), shadow));

}


[backpack]
void main()
{
    vec3 normal = lx_NormalMap(normalMap,fs_in.texCoords);
    float shadow = lx_ShadowCalculation(depthMap, fs_in.fragPos, light.position, cameraPos, farPlane);
    lx_FragColour = lx_Colour(lx_Phong(normal, fs_in.TBNfragPos, fs_in.TBNcameraPos, fs_in.texCoords, fs_in.texCoords, material, lx_MoveLight(light,fs_in.TBNlightPos), shadow));
}

[light]
void main()
{
    lx_FragColour = vec4(1.0);
}