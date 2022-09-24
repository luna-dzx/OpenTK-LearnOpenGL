using OpenTK.Graphics.OpenGL4;

namespace Library;

public class PostProcessing
{
    public static void Draw() => GL.DrawArrays(PrimitiveType.Triangles,0,3);
}