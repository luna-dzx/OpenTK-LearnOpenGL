#version luma-dx
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in mat4 instanceMatrix;

out VS_OUT {
    vec2 texCoords;
    vec4 TBNfragPos;
    mat3 tbnMatrix;
} vs_out;


uniform vec3 cameraPos;

void main()
{
    mat4 modelMatrix = instanceMatrix * lx_Model;

    mat3 TBN = lx_TBN(modelMatrix,aTangent,aNormal);

    vs_out.texCoords = aTexCoords;
    vs_out.TBNfragPos.xyz = TBN * (modelMatrix*vec4(aPos,1.0)).xyz;
    vs_out.tbnMatrix = TBN;

    gl_Position = lx_Proj * lx_View * modelMatrix * vec4(aPos, 1.0);
    vs_out.TBNfragPos.w = gl_Position.z;

}