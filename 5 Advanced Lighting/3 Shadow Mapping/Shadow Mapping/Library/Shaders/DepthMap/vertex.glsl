#version luma-dx
layout (location = 0) in vec3 aPos;

uniform mat4 lightSpaceMatrix;

void main() { gl_Position = lightSpaceMatrix*lx_Model*vec4(aPos, 1.0); }