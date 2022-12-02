#version luma-dx

uniform lx_Material material;
uniform sampler2D normalMap;

layout (location = 0) out vec4 gPosition;
layout (location = 1) out vec3 gNormalMap;
layout (location = 2) out vec4 gAlbedoSpec;
layout (location = 3) out vec3 tbnColumn0;
layout (location = 4) out vec3 tbnColumn1;
layout (location = 5) out vec3 tbnColumn2;

in VS_OUT {
    vec2 texCoords;
    vec4 TBNfragPos;
    mat3 tbnMatrix;
} fs_in;


void main()
{
    gPosition = fs_in.TBNfragPos;
    gNormalMap = lx_NormalMap(normalMap,fs_in.texCoords);
    gAlbedoSpec.rgb = texture(material.baseTex,fs_in.texCoords).rgb;
    gAlbedoSpec.a = texture(material.specTex,fs_in.texCoords).r;
    
    tbnColumn0 = fs_in.tbnMatrix[0];
    tbnColumn1 = fs_in.tbnMatrix[1];
    tbnColumn2 = fs_in.tbnMatrix[2];
}