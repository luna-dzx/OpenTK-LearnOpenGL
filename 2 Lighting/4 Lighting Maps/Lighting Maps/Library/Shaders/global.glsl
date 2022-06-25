#version 330 core

struct lx_Material 
{
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
};

struct lx_TexMaterial
{
    sampler2D diffuse;
    vec3 specular;
    float shininess;
};


struct lx_Light {
    vec3 position;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};