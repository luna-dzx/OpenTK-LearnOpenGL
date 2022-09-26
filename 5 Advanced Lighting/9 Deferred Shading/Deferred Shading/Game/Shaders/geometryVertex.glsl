#version luma-dx
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;

uniform mat4 modelMatrices[50];

out VS_OUT {
    vec2 texCoords;
    vec4 TBNfragPos;
    mat3 tbnMatrix;
} vs_out;


uniform vec3 cameraPos;

void main()
{
    mat3 TBN = lx_TBN(modelMatrices[gl_InstanceID],aTangent,aNormal);

    vs_out.texCoords = aTexCoords;
    vs_out.TBNfragPos.xyz = TBN * (modelMatrices[gl_InstanceID]*vec4(aPos,1.0)).xyz;
    vs_out.tbnMatrix = TBN;

    gl_Position = lx_Proj * lx_View * modelMatrices[gl_InstanceID] * vec4(aPos, 1.0);
    vs_out.TBNfragPos.w = gl_Position.z;

}