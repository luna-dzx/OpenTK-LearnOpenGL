#version luma-dx
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;

out VS_OUT {
    vec3 fragPos;
    vec3 normal;
} vs_out;


uniform vec3 cameraPos;

void main()
{
    vec4 viewPos = lx_View * lx_Model * vec4(aPos, 1.0);
    vs_out.fragPos = viewPos.xyz;
    
    mat3 normalMatrix = transpose(inverse(mat3(lx_View * lx_Model)));
    vs_out.normal = normalMatrix * aNormal;

    gl_Position = lx_Proj * viewPos;
}