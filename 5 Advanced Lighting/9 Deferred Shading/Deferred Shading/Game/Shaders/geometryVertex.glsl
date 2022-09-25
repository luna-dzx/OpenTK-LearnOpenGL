#version luma-dx
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;


out VS_OUT {
    vec2 texCoords;
    vec3 TBNfragPos;
    mat3 tbnMatrix;
} vs_out;


uniform lx_Light light;
uniform vec3 cameraPos;

void main()
{
    mat3 TBN = lx_TBN(lx_Model,aTangent,aNormal);

    vs_out.texCoords = aTexCoords;
    vs_out.TBNfragPos = TBN * (lx_Model*vec4(aPos,1.0)).xyz;
    vs_out.tbnMatrix = TBN;

    gl_Position = lx_Transform * vec4(aPos, 1.0);

}