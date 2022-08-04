out vec4 lx_FragColour;

vec3 lx_BasePhong(in vec3 normal, in vec3 fragPos, in vec3 cameraPos, in vec2 texCoords, in vec2 specTexCoords, in int textureMode, in lx_Material material, in lx_Light light)
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
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * material.specular * specTexSample;
    
    // fix buggy lighting
    if (abs(diffuse.x)+abs(diffuse.y)+abs(diffuse.z) == 0){specular*=0;}

    ambient  *= attenuation; 
    diffuse  *= attenuation * intensity;
    specular *= attenuation * intensity;

    return ambient + diffuse + specular; 
}

vec3 lx_Phong(in vec3 normal, in vec3 fragPos, in vec3 cameraPos, in lx_Material material, in lx_Light light)
{
    return lx_BasePhong(normal,fragPos,cameraPos,vec2(0.0),vec2(0.0),0,material,light);
}
vec3 lx_Phong(in vec3 normal, in vec3 fragPos, in vec3 cameraPos, in vec2 texCoords, in lx_Material material, in lx_Light light)
{
    return lx_BasePhong(normal,fragPos,cameraPos,texCoords,vec2(0.0),1,material,light);
}
vec3 lx_Phong(in vec3 normal, in vec3 fragPos, in vec3 cameraPos, in vec2 texCoords, in vec2 specTexCoords, in lx_Material material, in lx_Light light)
{
    return lx_BasePhong(normal,fragPos,cameraPos,texCoords,specTexCoords,2,material,light);
}