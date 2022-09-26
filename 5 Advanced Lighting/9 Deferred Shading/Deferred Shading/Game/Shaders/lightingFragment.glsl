#version luma-dx

uniform lx_Material material;
uniform lx_Light lights[64];

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

    if (texture(gPosition,texCoords).a == 0f)
    {
        discard;
    }
    else
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
        }
    
        
        lx_FragColour = vec4(lighting, 1.0);
    }
    
    
}