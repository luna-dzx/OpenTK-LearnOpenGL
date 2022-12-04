#version luma-dx

uniform lx_Material material;
uniform sampler2D normalMap;

layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 gNormal;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
} fs_in;


void main()
{
    // store the fragment position vector in the first gbuffer texture
    gPosition = fs_in.fragPos;
    // also store the per-fragment normals into the gbuffer
    gNormal = normalize(fs_in.normal);
}