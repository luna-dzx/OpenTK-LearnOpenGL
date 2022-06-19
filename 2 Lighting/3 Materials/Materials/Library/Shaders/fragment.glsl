out vec4 lx_FragColour;

// TODO: redo these functions for the new structs

float lx_ViewPhong(vec3 lx_fragPos, vec3 lx_lightPos, vec3 lx_normal, float lx_specularStrength, int lx_specularExponent)
{
    vec3 lx_lightDir = normalize(lx_lightPos - lx_fragPos);

    float lx_diffuse = max(dot(lx_normal,lx_lightDir),0.0);
    
    vec3 lx_viewDir = normalize(-lx_fragPos);
    vec3 lx_reflectDir = reflect(-lx_lightDir,lx_normal);

    float lx_specular = lx_specularStrength * pow(max(dot(lx_viewDir, lx_reflectDir), 0.0), lx_specularExponent);
    
    return lx_specular + lx_diffuse;
}

float lx_Phong(vec3 lx_fragPos, vec3 lx_lightPos, vec3 lx_viewPos, vec3 lx_normal, float lx_specularStrength, int lx_specularExponent)
{
    vec3 lx_lightDir = normalize(lx_lightPos - lx_fragPos);

    float lx_diffuse = max(dot(lx_normal,lx_lightDir),0.0);
    
    vec3 lx_viewDir = normalize(lx_viewPos-lx_fragPos);
    vec3 lx_reflectDir = reflect(-lx_lightDir,lx_normal);

    float lx_specular = lx_specularStrength * pow(max(dot(lx_viewDir, lx_reflectDir), 0.0), lx_specularExponent);
    
    return lx_specular + lx_diffuse;
}