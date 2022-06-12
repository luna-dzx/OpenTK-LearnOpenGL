#version luma-dx

uniform vec3 objectColour;
uniform vec3 lightColour;

// view space vectors (where camera position is the origin)
in vec3 vNormal;
in vec3 vFragPos;
in vec3 vLightPos;

[shader]

void main()
{
    // ambient
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * lightColour;
    
    // diffuse
    vec3 norm = normalize(vNormal);
    vec3 lightDir = normalize(vLightPos - vFragPos);
    
    float diff = max(dot(norm,lightDir),0.0);
    vec3 diffuse = diff * lightColour;
    
    // specular
    
    float specularStrength = 0.5;
    vec3 viewDir = normalize(-vFragPos);
    vec3 reflectDir = reflect(-lightDir,norm); // lightDir is negative to make this point towards the light
    
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * lightColour;
    
    
    vec3 result = (ambient+diffuse+specular) * objectColour;
    lx_FragColour = vec4(result,1.0);
}


[lightShader]

void main()
{
    lx_FragColour = vec4(lightColour,1.0);
}