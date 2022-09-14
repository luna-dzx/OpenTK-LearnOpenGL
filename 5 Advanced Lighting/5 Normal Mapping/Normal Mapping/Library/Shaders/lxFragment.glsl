out vec4 lx_FragColour;

uniform bool lx_IsGammaCorrectionEnabled;

vec4 lx_GammaCorrect(vec4 colour, float gamma)
{
    return vec4(pow(colour.rgb, vec3(gamma)),colour.w);
}
vec3 lx_GammaCorrect(vec3 colour, float gamma)
{
    return pow(colour, vec3(gamma));
}

float lx_Diffuse(in vec3 normal, in vec3 fragPos, in vec3 lightPos)
{
    return max(dot(normal, lightPos - fragPos), 0.0);
}

vec3 lx_BasePhong(in vec3 normal, in vec3 fragPos, in vec3 cameraPos, in vec2 texCoords, in vec2 specTexCoords, in int textureMode, in lx_Material material, in lx_Light light, float lightMult)
{
    normal = normalize(normal);

    vec3 baseTexSample = vec3(1.0);
    vec3 specTexSample = vec3(1.0);

    if (textureMode > 0)
    {
        baseTexSample = vec3(texture(material.baseTex, texCoords));
    }

    if (textureMode > 1)
    {
        specTexSample = vec3(texture(material.specTex, specTexCoords));
    }
    
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

vec3 lx_Phong(in vec3 normal, in vec3 fragPos, in vec3 cameraPos, in lx_Material material, in lx_Light light, float lightMult)
{
    return lx_BasePhong(normal,fragPos,cameraPos,vec2(0.0),vec2(0.0),0,material,light,lightMult);
}
vec3 lx_Phong(in vec3 normal, in vec3 fragPos, in vec3 cameraPos, in vec2 texCoords, in lx_Material material, in lx_Light light, float lightMult)
{
    return lx_BasePhong(normal,fragPos,cameraPos,texCoords,vec2(0.0),1,material,light,lightMult);
}
vec3 lx_Phong(in vec3 normal, in vec3 fragPos, in vec3 cameraPos, in vec2 texCoords, in vec2 specTexCoords, in lx_Material material, in lx_Light light, float lightMult)
{
    return lx_BasePhong(normal,fragPos,cameraPos,texCoords,specTexCoords,2,material,light,lightMult);
}

float lx_ShadowCalculation(sampler2D shadowTexture, vec4 fragPosLightSpace)
{
    vec3 projCoords = (fragPosLightSpace.xyz / fragPosLightSpace.w) * 0.5 + 0.5;
    float closestDepth = texture(shadowTexture, projCoords.xy).r;
    float currentDepth = projCoords.z;
    float textureDepth = texture(shadowTexture, projCoords.xy).r;
    
    float shadow = (currentDepth > textureDepth ? 1.0 : 0.0);

    if(projCoords.z > 1.0)
        shadow = 0.0;
        
    return 1.0-shadow;
}

float lx_ShadowCalculation(sampler2D shadowTexture, vec4 fragPosLightSpace, float texelOffset, int range, float divisor)
{
    vec3 projCoords = (fragPosLightSpace.xyz / fragPosLightSpace.w) * 0.5 + 0.5;
    float closestDepth = texture(shadowTexture, projCoords.xy).r;
    float currentDepth = projCoords.z;
    float textureDepth = texture(shadowTexture, projCoords.xy).r;
    
    float shadow = (currentDepth > textureDepth ? 1.0 : 0.0);
    

    if (shadow == 0.0)
    {
        vec2 texelSize = texelOffset / textureSize(shadowTexture, 0);
        for(int x = -range; x <= range; x++)
        {
            for(int y = -range; y <= range; y++)
            {
                float pcfDepth = texture(shadowTexture, projCoords.xy + vec2(x, y) * texelSize).r; 
                shadow += currentDepth > pcfDepth ? 1.0 : 0.0;
            }    
        }
        shadow = min(shadow/divisor,0.98);
    }
    
    if(projCoords.z > 1.0)
        shadow = 0.0;
        
        
    return 1.0-shadow;
}

vec4 lx_MultiSample(sampler2DMS sampler, ivec2 texCoords, int numSamples)
{
    vec4 pixelColour = texelFetch(sampler, texCoords, 0);
    for(int i = 1; i < numSamples; i++)
    {
        pixelColour += texelFetch(sampler, texCoords, i);
    }
    
    return pixelColour / float(numSamples);
}

[main]
lx_FragColour = vec4(0.0);

[post-main]
if (lx_IsGammaCorrectionEnabled)
{
    lx_FragColour = lx_GammaCorrect( lx_FragColour , 1.0/2.2);
}
