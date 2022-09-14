#version luma-dx
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec3 aTangent;


out VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
    vec3 TBNfragPos;
    vec3 TBNlightPos;
    vec3 TBNcameraPos;
    vec3 tangent;
} vs_out;

uniform lx_Light light;
uniform vec3 cameraPos;

/*
out VS_OUT {
    mat3 TBN;
} vs_out;*/

void main()
{
    vec3 T = normalize( mat3(lx_Model) * aTangent );
    vec3 N = normalize( mat3(lx_Model) * aNormal );
   
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);
   
    mat3 TBN = transpose(mat3(T, B, N));

    
    vs_out.normal = lx_NormalFix(lx_Model,aNormal);
    vs_out.texCoords = aTexCoords;
    vs_out.fragPos = (lx_Model*vec4(aPos,1.0)).xyz;
    
    vs_out.TBNfragPos = TBN * vs_out.fragPos;
    vs_out.TBNcameraPos = TBN * cameraPos;
    vs_out.TBNlightPos = TBN * light.position;
    vs_out.tangent = TBN * vec3(1,0,0);
    
    //vs_out.TBN = TBN;

    gl_Position = lx_Transform * vec4(aPos, 1.0);

}