out vec4 lx_FragColour;

// viewspace phong lighting calculations
vec3 lx_vPhong(vec3 vNormal, vec3 vFragPos, lx_Material material, lx_Light light)
{
    vec3 ambient = light.ambient * material.ambient;
    
    vec3 lightDir = normalize(light.position - vFragPos);
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