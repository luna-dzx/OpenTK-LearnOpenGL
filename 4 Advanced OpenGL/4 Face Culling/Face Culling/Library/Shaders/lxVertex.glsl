uniform mat4 lx_Model;
uniform mat4 lx_View;
uniform mat4 lx_Proj;
uniform int lx_AutoProjection;
mat4 lx_Transform = mat4(1.0);

[main]
if (lx_AutoProjection == 1){lx_Transform = lx_Proj * lx_View * lx_Model;}