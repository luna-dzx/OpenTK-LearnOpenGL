#version luma-dx

uniform lx_Material material;
uniform lx_Light lights[NUM_LIGHTS];

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
    vec4 inColour = texture(gPosition,texCoords);
    lx_DiscardBackground(inColour);
    
    vec3 fragPos = inColour.rgb;
    vec3 normal = texture(gNormal,texCoords).rgb;
    vec3 albedo = texture(gAlbedoSpec,texCoords).rgb;
    float specular = texture(gAlbedoSpec,texCoords).a;
    
    mat3 TBN = lx_ConstructMatrix(
        texture(tbnColumn0,texCoords).rgb,
        texture(tbnColumn1,texCoords).rgb,
        texture(tbnColumn2,texCoords).rgb
    );
    
    lx_FragColour = lx_Colour(lx_DeferredPhong(normal,fragPos,TBN*cameraPos,albedo,specular,material,lights,NUM_LIGHTS,TBN,1.0));
}