using System;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace Library;

/// <summary>
/// More abstracted way of handling VAOs - dependent on the VAO class.
/// This class is more geared towards very simple VAO usage, and is a
/// faster way of creating the "general case" of simply loading a mesh
/// </summary>
public class Model : VertexArray
{
    private Objects.Mesh mesh;

    private int uTransform;
    private Matrix4 transform = Matrix4.Identity;

    public float[]? GetVertices => mesh.Vertices;
    public float[]? GetTexCoords => mesh.TexCoords;
    public float[]? GetNormals => mesh.Normals;
    public int[]? GetIndices => mesh.Indices;

    private PrimitiveType renderMode;


    /// <summary>
    /// Create a new model, which contains an empty mesh object and a transformation matrix
    /// </summary>
    /// <param name="modelMatrixBinding">uniform mat4 model glsl binding</param>
    /// <param name="primitiveType">the primitive type to render this vao with (when draw is called)</param>
    public Model(int modelMatrixBinding = -1, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        mesh = new Objects.Mesh();
        uTransform = modelMatrixBinding;
        renderMode = primitiveType;
    }

    /// <summary>
    /// Create a new model, then load vertex data to the vao and mesh object
    /// </summary>
    /// <param name="vertices">vertex data to load</param>
    /// <param name="vertexBinding">glsl vertex binding to load the vertex data to in the vertex shader</param>
    /// <param name="modelMatrixBinding">uniform mat4 model glsl binding</param>
    /// <param name="primitiveType">the primitive type to render this vao with (when draw is called)</param>
    public Model(float[] vertices, int vertexBinding = 0, int modelMatrixBinding = -1, PrimitiveType primitiveType = PrimitiveType.Triangles)
        :this(modelMatrixBinding,primitiveType)
    {
        LoadVertices(vertexBinding,vertices);
    }
        
    /// <summary>
    /// Create a new model, then load vertex data to the vao and mesh object, as well as lading index data for rendering
    /// </summary>
    /// <param name="vertices">vertex data to load</param>
    /// <param name="indices">index data for rendering</param>
    /// <param name="vertexBinding">glsl vertex binding to load the vertex data to in the vertex shader</param>
    /// <param name="modelMatrixBinding">uniform mat4 model glsl binding</param>
    /// <param name="primitiveType">the primitive type to render this vao with (when draw is called)</param>
    public Model(float[] vertices, int[] indices, int vertexBinding = 0, int modelMatrixBinding = -1, PrimitiveType primitiveType = PrimitiveType.Triangles)
        :this(vertices,vertexBinding,modelMatrixBinding,primitiveType)
    {
        LoadIndices(indices);
    }

    /// <summary>
    /// Create a model from a pre-existing mesh
    /// </summary>
    /// <param name="meshData">Mesh to load (loads all data to VAO)</param>
    /// <param name="modelMatrixBinding">uniform mat4 model glsl binding</param>
    /// <param name="primitiveType">the primitive type to render this vao with (when draw is called)</param>
    public Model(Objects.Mesh meshData, int modelMatrixBinding = -1, PrimitiveType primitiveType = PrimitiveType.Triangles)
        :this(modelMatrixBinding,primitiveType)
    {
        LoadMesh(meshData);
    }



    public static Model FromFile(
        string directory,
        string fileName,
        out Dictionary<TextureType,List<Texture>> textures,
        int modelMatrixBinding = -1,
        TextureType[] filters = null,
        PostProcessSteps postProcessFlags = PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs
        )
    {
        Scene scene; //"../../../../../../0 Assets/backpack/backpack.obj"
        AssimpContext importer = new AssimpContext();
        //importer.SetConfig(new Assimp.Configs.NormalSmoothingAngleConfig(66.0f));
        //Console.WriteLine("Loading Model...");
        scene = importer.ImportFile(directory+fileName, postProcessFlags);//,PostProcessPreset.TargetRealTimeMaximumQuality);
        //Console.WriteLine("Model Loaded");
        
        int vertexCount = scene.Meshes.Sum(mesh => mesh.VertexCount);
        int indexCount = scene.Meshes.Sum(mesh => mesh.Faces.Sum(face => face.IndexCount));


        textures = new();

        // load all textures of each type from all materials (yeah idk why the structure has to be this convoluted it's annoying)
        int offset = 0;
        foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
        {
            // filter out certain texture types because images take a while to load
            if (filters != null && !filters.Contains(type)) continue;

            foreach (var material in scene.Materials)
            {
                List<Texture> currentTextures = new List<Texture>();
        
                for (int j = 0; j < material.GetMaterialTextureCount(type); j++)
                {
                    material.GetMaterialTexture(type, j, out var slot);
                    string path = directory + slot.FilePath;
                    var texture = new Texture(path, (offset + slot.TextureIndex));
                    currentTextures.Add(texture);
                }

                offset += currentTextures.Count;
                if (!textures.ContainsKey(type))
                {
                    textures.Add(type,currentTextures);
                }
                else
                {
                    textures[type] = currentTextures;
                }
            }

        }
        
        
        var vertices = new float[vertexCount * 3];
        var normals = new float[vertexCount * 3];
        var texCoords = new float[vertexCount * 2];
        var indices = new int[indexCount];
        
        int indexOffset = 0;
        int vertexIndex = 0;

        foreach (Mesh mesh in scene.Meshes)
        {
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                vertices[vertexIndex*3] = mesh.Vertices[i].X;
                vertices[vertexIndex*3 +1] = mesh.Vertices[i].Y;
                vertices[vertexIndex*3 +2] = mesh.Vertices[i].Z;
                
                normals[vertexIndex*3] = mesh.Normals[i].X;
                normals[vertexIndex*3 +1] = mesh.Normals[i].Y;
                normals[vertexIndex*3 +2] = mesh.Normals[i].Z;
                
                texCoords[vertexIndex*2] = mesh.TextureCoordinateChannels[0][i].X;
                texCoords[vertexIndex*2 +1] = mesh.TextureCoordinateChannels[0][i].Y;

                vertexIndex++;
            }

            int numIndices = 0;
            foreach (Face face in mesh.Faces)
            {
                foreach (int index in face.Indices)
                {
                    indices[indexOffset+numIndices] = indexOffset+index;
                    numIndices++;
                }
            }

            indexOffset += numIndices;
        }
        
        
        var finalMesh = new Objects.Mesh(
            vertices: vertices,
            indices: indices,
            texCoords: texCoords,
            normals: normals
        );
        
        return new Model(finalMesh, modelMatrixBinding);
        
    }
    
    

    /// <summary>
    /// Loads all mesh data to the VAO
    /// </summary>
    /// <param name="meshData">the mesh to load from</param>
    /// <returns>current object for ease of use</returns>
    public Model LoadMesh(Objects.Mesh meshData)
    {
        if (meshData.Vertices != null) Add(meshData.VertexBinding, meshData.Vertices);
        if (meshData.TexCoords != null) Add(meshData.TexCoordBinding, meshData.TexCoords, BufferTarget.ArrayBuffer, 2, 2);
        if (meshData.Normals != null) Add(meshData.NormalBinding, meshData.Normals);
        if (meshData.Indices != null) StoreData(meshData.Indices, BufferTarget.ElementArrayBuffer);

        mesh = meshData;
        return this;
    }

    /// <summary>
    /// Load vertex data to the VAO
    /// </summary>
    /// <param name="layoutLocation">the glsl binding that the data will be sent to</param>
    /// <param name="vertices">vertex data to load</param>
    /// <returns>current object for ease of use</returns>
    public Model LoadVertices(int layoutLocation,float[] vertices)
    {
        mesh.Vertices = vertices;
        Add(layoutLocation, mesh.Vertices);
        return this;
    }

    /// <summary>
    /// Loads indices to the VAO for rendering vertex data
    /// </summary>
    /// <param name="indices">index data to load</param>
    /// <returns>current object for ease of use</returns>
    public Model LoadIndices(int[] indices)
    {
        mesh.Indices = indices; 
        StoreData(mesh.Indices, BufferTarget.ElementArrayBuffer);
        return this;
    }

    /// <summary>
    /// Loads texture coordinates to the VAO
    /// </summary>
    /// <param name="layoutLocation">the glsl binding that the data will be sent to</param>
    /// <param name="texCoords">the texture coordinates to load</param>
    /// <returns>current object for ease of use</returns>
    public Model LoadTexCoords(int layoutLocation, float[] texCoords)
    {
        mesh.TexCoords = texCoords;
        Add(layoutLocation, mesh.TexCoords, BufferTarget.ArrayBuffer, 2, 2);
        return this;
    }

    /// <summary>
    /// Loads per vertex normal data to the VAO
    /// </summary>
    /// <param name="layoutLocation">the glsl binding that the data will be sent to</param>
    /// <param name="normals">normals data to load</param>
    /// <returns>current object for ease of use</returns>
    public Model LoadNormals(int layoutLocation, float[] normals)
    {
        mesh.Normals = normals;
        Add(layoutLocation, normals);
        return this;
    }

    /// <summary>
    /// Change how the vertex data is rendered
    /// </summary>
    /// <param name="primitiveType">defines the shape that is drawn on calling the draw function</param>
    /// <remarks>primitive type is triangles by default</remarks>
    public void SetPrimitiveType(PrimitiveType primitiveType) => renderMode = primitiveType;

    /// <summary>
    /// Load the transformation matrix onto the GPU
    /// </summary>
    public void UpdateTransformation(Matrix4 transformation)
    {
        GL.UniformMatrix4(uTransform,false,ref transformation);
    }

    /// <summary>
    /// Set a new transformation matrix and load it to the gpu
    /// </summary>
    /// <param name="translation">position relative to the origin</param>
    /// <param name="rotation">rotation in the x,y and z axis</param>
    /// <param name="scale">scale in x,y and z</param>
    public Model Transform(Vector3 translation, Vector3 rotation, Vector3 scale)
    {
        transform = Maths.CreateTransformation(translation, rotation, scale);
        UpdateTransformation(transform);
        return this;
    }

    /// <summary>
    /// Set a new transformation matrix and load it to the gpu
    /// </summary>
    /// <param name="translation">position relative to the origin</param>
    /// <param name="rotation">rotation in the x,y and z axis</param>
    /// <param name="scale">scale of the overall object in all 3 dimensions</param>
    public Model Transform(Vector3 translation, Vector3 rotation, float scale)
    {
        transform = Maths.CreateTransformation(translation, rotation, new Vector3(scale,scale,scale));
        UpdateTransformation(transform);
        return this;
    }



    /// <summary>
    /// Multiply the current object's scale by this value and load the model matrix to the gpu
    /// </summary>
    /// <param name="scale">scale in x,y and z</param>
    public void Scale(Vector3 scale)
    {
        transform = Matrix4.CreateScale(scale) * transform;
        UpdateTransformation(transform);
    }

    /// <summary>
    /// Multiply the current object's scale by this value and load the model matrix to the gpu
    /// </summary>
    /// <param name="scale">scale of the overall object in all 3 dimensions</param>
    public void Scale(float scale)
    {
        transform = Matrix4.CreateScale(scale,scale,scale) * transform;
        UpdateTransformation(transform);
    }


    /// <summary>
    /// Sets the transform to default (at origin, no rotation, scale 1) and load the model matrix to the gpu
    /// </summary>
    public Model ResetTransform()
    {
        transform = Matrix4.Identity;
        UpdateTransformation(transform);
        return this;
    }


    /// <summary>
    /// Draw the object based on the current configuration
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Draw()
    {
        Use();

        if (mesh.Indices != null && mesh.Indices.Length != 0)
        {
            GL.DrawElements(renderMode,mesh.Indices.Length,DrawElementsType.UnsignedInt,0); return;
        }
        if (mesh.Vertices != null && mesh.Vertices.Length != 0)
        {
            GL.DrawArrays(renderMode,0,mesh.Vertices.Length/3); return;
        }

        throw new Exception("Invalid Mesh");


    }

}