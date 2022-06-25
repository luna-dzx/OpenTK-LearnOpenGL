out vec4 lx_FragColour;

// viewspace phong lighting calculations
vec3 lx_vPhong(vec3 vNormal, vec3 vFragPos, vec3 vLightPos, lx_Material material, lx_Light light)
{
    vec3 ambient = light.ambient * material.ambient;
    
    vec3 lightDir = normalize(vLightPos - vFragPos);
    float diff = max(dot(vNormal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * (diff * material.diffuse);
    
    vec3 viewDir = normalize(-vFragPos);
    vec3 reflectDir = reflect(-lightDir, vNormal);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * (spec * material.specular);  
    
    // fix buggy lighting
    if (abs(diffuse.x)+abs(diffuse.y)+abs(diffuse.z) == 0){specular*=0;}

    return ambient + diffuse + specular; 
}

vec3 lx_vPhong(vec3 vNormal, vec3 vFragPos, vec3 vLightPos, vec2 texCoords, lx_TexMaterial material, lx_Light light)
{
    vec3 textureSample = vec3(texture(material.baseTex, texCoords));

    vec3 ambient = light.ambient * material.ambient * textureSample;
    
    vec3 lightDir = normalize(vLightPos - vFragPos);
    float diff = max(dot(vNormal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * material.diffuse * textureSample;
    
    vec3 viewDir = normalize(-vFragPos);
    vec3 reflectDir = reflect(-lightDir, vNormal);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * material.specular;  
    
    // fix buggy lighting
    if (abs(diffuse.x)+abs(diffuse.y)+abs(diffuse.z) == 0){specular*=0;}

    return ambient + diffuse + specular; 
}

vec3 lx_vPhong(vec3 vNormal, vec3 vFragPos, vec3 vLightPos, vec2 texCoords, lx_SpecTexMaterial material, lx_Light light)
{
    vec3 baseTexSample = vec3(texture(material.baseTex, texCoords));
    vec3 specTexSample = vec3(texture(material.specTex, texCoords));

    vec3 ambient = light.ambient * material.ambient * baseTexSample;
    
    vec3 lightDir = normalize(vLightPos - vFragPos);
    float diff = max(dot(vNormal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * material.diffuse * baseTexSample;
    
    vec3 viewDir = normalize(-vFragPos);
    vec3 reflectDir = reflect(-lightDir, vNormal);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * material.specular * specTexSample;  
    
    // fix buggy lighting
    if (abs(diffuse.x)+abs(diffuse.y)+abs(diffuse.z) == 0){specular*=0;}

    return ambient + diffuse + specular; 
}