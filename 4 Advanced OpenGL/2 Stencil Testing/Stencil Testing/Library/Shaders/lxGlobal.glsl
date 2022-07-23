#version 330 core

struct lx_Material
{
    sampler2D baseTex;
    sampler2D specTex;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};

struct lx_Light {
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    vec3 attenuation;
};

vec3 lx_NormalFix(mat4 appliedMatrix, vec3 normal)
{
    return mat3(transpose(inverse(appliedMatrix))) * normal;
}