#version luma-dx

uniform vec3 objectColour;
uniform vec3 lightColour;

struct Material 
{
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};


struct Light {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};


uniform Material material;
uniform Light light;

// view space vectors (where camera position is the origin)
in vec3 vNormal;
in vec3 vFragPos;
in vec3 vLightPos;

[shader]

void main()
{
    vec3 ambient = light.ambient * material.ambient;
    
    vec3 lightDir = normalize(vLightPos - vFragPos);
    float diff = max(dot(vNormal, lightDir), 0.0);
    vec3 diffuse = light.diffuse * (diff * material.diffuse);
    
    vec3 viewDir = normalize(-vFragPos);
    vec3 reflectDir = reflect(-lightDir, vNormal);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * (spec * material.specular);  

    vec3 result = ambient + diffuse + specular;
    lx_FragColour = vec4(result,1.0);
}


[lightShader]

void main()
{
    lx_FragColour = vec4(lightColour,1.0);
}