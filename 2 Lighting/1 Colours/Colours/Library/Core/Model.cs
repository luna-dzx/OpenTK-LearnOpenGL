using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Library;

public enum DrawType
{
    None,
    VertexArray,
    ElementArray
}

/// <summary>
/// More abstracted way of handling VAOs - dependent on the VAO class.
/// This class is more geared towards very simple VAO usage, and is a
/// faster way of creating the "general case" of simply loading a mesh
/// </summary>
public class Model : VertexArray
{
    private float[]? _vertices;
    private float[]? _texCoords;
    private int[]? _indices;

    private int uTransform;
    private Matrix4 transform = Matrix4.Identity;

    public float[]? GetVertices => _vertices;
    public float[]? GetTexCoords => _texCoords;
    public int[]? GetIndices => _indices;

    private DrawType drawType = DrawType.None;
    private PrimitiveType renderMode;


    public Model(int modelMatrixBinding = -1, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        uTransform = modelMatrixBinding;
        renderMode = primitiveType;
    }

    public void LoadVertices(int layoutLocation,float[] vertices)
    {
        _vertices = vertices;
        Add(layoutLocation, this._vertices);
        if (drawType == DrawType.None) drawType = DrawType.VertexArray;
    }

    public void LoadIndices(int[] indices)
    {
        _indices = indices; 
        StoreData(this._indices, BufferTarget.ElementArrayBuffer);
        drawType = DrawType.ElementArray;
    }

    public void LoadTexCoords(int layoutLocation, float[] texCoords)
    {
        _texCoords = texCoords;
        Add(layoutLocation, this._texCoords, BufferTarget.ArrayBuffer, 2, 2);
    }

    public void SetPrimitiveType(PrimitiveType primitiveType) => renderMode = primitiveType;

    public void UpdateTransformation(Vector3 translation, Vector3 rotation, Vector3 scale)
    {
        transform = Maths.CreateTransformation(translation, rotation, scale);
    }

    public void Draw()
    {
        Use();

        if (uTransform != -1)
        {
            GL.UniformMatrix4(uTransform,false,ref transform);
        }

        switch (drawType)
        {
            case DrawType.VertexArray:
                GL.DrawArrays(renderMode,0,_vertices.Length/3); break;
            case DrawType.ElementArray:
                GL.DrawElements(renderMode,_indices.Length,DrawElementsType.UnsignedInt,0); break;
            case DrawType.None: default:
                throw new Exception("No data to draw");
        }

    }

}