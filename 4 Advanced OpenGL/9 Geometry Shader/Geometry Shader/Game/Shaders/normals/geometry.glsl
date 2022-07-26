#version luma-dx

layout (triangles) in;
layout (line_strip, max_vertices = 6) out;

in VS_OUT {
    vec3 normal;
} gs_in[];

const float MAGNITUDE = 0.4;
  
uniform mat4 lx_Proj;

void main()
{
    
    vec4 position = vec4(0.0);
    vec3 normal = vec3(0.0);
    
    for(int i = 0; i < 3; i++)
    {
        position += gl_in[i].gl_Position / 3.0;
        normal += gs_in[i].normal / 3.0;
    }
    

    gl_Position = lx_Proj * position;
    EmitVertex();
    gl_Position = lx_Proj * (position + vec4(normal, 0.0) * MAGNITUDE);
    EmitVertex();
    EndPrimitive();
}