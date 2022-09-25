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

void main()
{
    vec3 fragPos = texture(gPosition,texCoords).rgb;
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
    
    lx_Light tbnLight = light;
    tbnLight.position = TBN*tbnLight.position;
    
    vec3 lighting = albedo * 0.1; // hard-coded ambient component
    vec3 viewDir = normalize(tbnCameraPos - fragPos);
        
    vec3 lightDir = normalize(tbnLight.position - fragPos);
    vec3 diffuse = max(dot(normal, lightDir), 0.0) * albedo * tbnLight.diffuse;
    lighting += diffuse;

    
    lx_FragColour = vec4(lighting, 1.0);
}