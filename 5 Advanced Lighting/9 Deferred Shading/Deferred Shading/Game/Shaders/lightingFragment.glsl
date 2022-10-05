#version luma-dx

uniform lx_Material material;
uniform lx_Light light;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedoSpec;
uniform sampler2D tbnColumn0;
uniform sampler2D tbnColumn1;
uniform sampler2D tbnColumn2;

uniform vec3 cameraPos;

in vec2 texCoords;



vec3 lx_DeferredPhong(in vec3 normal, in vec3 fragPos, in vec3 cameraPos, in vec3 albedoInput, in float specularInput, in lx_Material material, in lx_Light light, float lightMult)
{
    normal = normalize(normal);

    vec3 baseTexSample = albedoInput;
    vec3 specTexSample = vec3(specularInput);

    
    if (lx_IsGammaCorrectionEnabled)
    {
        baseTexSample = lx_GammaCorrect(baseTexSample,2.2);
        specTexSample = lx_GammaCorrect(specTexSample,2.2);
    }

    vec3 ambient = light.ambient * material.ambient * baseTexSample;
    
    vec3 lightDir = vec3(0.0);
    
    if (light.cutOff > 0)
    {
        lightDir = light.position - fragPos;
    }
    else
    {
        lightDir = -light.direction;
    }
    
    float distance = length(lightDir);
    float attenuation = 1.0 / dot(light.attenuation,vec3(1.0,distance,distance*distance));
    
    lightDir = lightDir/distance;
    
    float intensity = 1.0;
    
    if (light.cutOff < 1 && light.cutOff > 0) // if this light is a spotlight
    {
        float cosTheta = dot(lightDir, normalize(-light.direction));
        
        if (light.outerCutOff > 0) // fade out at edge of splotlight
        {
            intensity = clamp((cosTheta - light.outerCutOff) / (light.cutOff - light.outerCutOff), 0.0, 1.0);
        }
        
        if(cosTheta < light.outerCutOff) // if angle > cutOff, but since it's cosine(angle), we use less than (cosine graph initially decreases)
        {
            return ambient;
        }
    }
    
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * material.diffuse * baseTexSample;
    
    vec3 viewDir = normalize(cameraPos - fragPos);
    
    //vec3 reflectDir = reflect(-lightDir, normal);
    //float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), material.shininess);
    
    vec3 specular = diff * light.specular * spec * material.specular * specTexSample;
    
    // fix buggy lighting
    if (abs(diffuse.x)+abs(diffuse.y)+abs(diffuse.z) == 0){specular*=0;}

    ambient  *= attenuation; 
    diffuse  *= attenuation * intensity;
    specular *= attenuation * intensity;

    return ambient + lightMult * (diffuse + specular); 
}




void main()
{

    vec4 inColour = texture(gPosition,texCoords);

    lx_DiscardBackground(inColour);
    
    vec3 fragPos = inColour.rgb;
    vec3 normal = texture(gNormal,texCoords).rgb;
    vec3 albedo = texture(gAlbedoSpec,texCoords).rgb;
    float specular = texture(gAlbedoSpec,texCoords).a;

    vec3 matrixColumn0 = texture(tbnColumn0,texCoords).rgb;
    vec3 matrixColumn1 = texture(tbnColumn1,texCoords).rgb;
    vec3 matrixColumn2 = texture(tbnColumn2,texCoords).rgb;
    
    mat3 TBN = mat3(0);
    TBN[0] = matrixColumn0;
    TBN[1] = matrixColumn1;
    TBN[2] = matrixColumn2;
    
    vec3 tbnCameraPos = TBN*cameraPos;
    

    /*
    vec3 lighting = albedo * 0.1; // hard-coded ambient component
    
    for(int i = 0; i < 64; ++i)
    {
        lx_Light tbnLight = lights[i];
        
        tbnLight.position = TBN*tbnLight.position;
        vec3 viewDir = normalize(tbnCameraPos - fragPos);

        vec3 lightDir = tbnLight.position - fragPos;
        
        float distance = length(lightDir);
        lightDir/=distance;
        
        float attenuation = 1.0 / dot(tbnLight.attenuation,vec3(1.0,distance,distance*distance));
        
        vec3 diffuse = max(dot(normal, lightDir), 0.0) * albedo * tbnLight.diffuse;
        lighting += diffuse*attenuation;
    }*/
    
    vec3 phong = lx_DeferredPhong(normal,fragPos,tbnCameraPos,albedo,specular,material,lx_MoveLight(light,TBN*light.position),1.0);
    
    lx_FragColour = lx_Colour(phong);
    
    //lx_FragColour = vec4(lighting, 1.0);
    
}