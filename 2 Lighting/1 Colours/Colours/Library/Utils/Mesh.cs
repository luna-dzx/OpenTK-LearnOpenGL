namespace Library;

/// <summary>
/// Stores data for a standard VAO
/// </summary>
public class Mesh
{
    public float[] Vertices;
    public float[] TexCoords;
    public float[] Normals;
    public int[] Indices;

    public int VertexBinding = 0;
    public int TexCoordBinding = 1;
    public int NormalBinding = 2;
        

    public Mesh(float[] vertices = null, int[] indices = null, float[] texCoords = null, float[] normals = null)
    {
        Vertices = vertices;
        Indices = indices;
        TexCoords = texCoords;
        Normals = normals;
    }
}
    
/// <summary>
/// Collection of standard meshes which have already been created
/// </summary>
public class PresetMesh
{

    public static readonly Mesh Cube = new Mesh
    (
        vertices: new float[]
        {
            // back face
            -1, -1, -1, 
            1, -1, -1, 
            1,  1, -1, 
            1,  1, -1, 
            -1,  1, -1, 
            -1, -1, -1, 

            // front face
            -1, -1,  1, 
            1, -1,  1, 
            1,  1,  1, 
            1,  1,  1, 
            -1,  1,  1, 
            -1, -1,  1, 

            // left face
            -1,  1,  1, 
            -1,  1, -1, 
            -1, -1, -1, 
            -1, -1, -1, 
            -1, -1,  1, 
            -1,  1,  1, 

            // right face
            1,  1,  1, 
            1,  1, -1, 
            1, -1, -1, 
            1, -1, -1, 
            1, -1,  1, 
            1,  1,  1, 

            // bottom face
            -1, -1, -1, 
            1, -1, -1, 
            1, -1,  1, 
            1, -1,  1, 
            -1, -1,  1, 
            -1, -1, -1, 

            // top face
            -1,  1, -1, 
            1,  1, -1, 
            1,  1,  1, 
            1,  1,  1, 
            -1,  1,  1, 
            -1,  1, -1, 
        },

        texCoords: new float[]
        {
            //back face
            0,0, 1,0, 1,1,
            1,1, 0,1, 0,0,
                
            // front face
            0,0, 1,0, 1,1,
            1,1, 0,1, 0,0,
                
            // left face
            1,0, 1,1, 0,1,
            0,1, 0,0, 1,0,
                
            // right face
            1,0, 1,1, 0,1,
            0,1, 0,0, 1,0,
                
            // bottom face
            0,1, 1,1, 1,0,
            1,0, 0,0, 0,1,
                
            // top face
            0,1, 1,1, 1,0,
            1,0, 0,0, 0,1,
        },
            
        normals: new float[]
        {
            //back face
            0,0,-1,
            0,0,-1,
            0,0,-1,
            0,0,-1,
            0,0,-1,
            0,0,-1,

            // front face
            0,0,1,
            0,0,1,
            0,0,1,
            0,0,1,
            0,0,1,
            0,0,1,
                
            // left face
            -1,0,0,
            -1,0,0,
            -1,0,0,
            -1,0,0,
            -1,0,0,
            -1,0,0,
                
            // right face
            1,0,0,
            1,0,0,
            1,0,0,
            1,0,0,
            1,0,0,
            1,0,0,
                
            // bottom face
            0,-1,0,
            0,-1,0,
            0,-1,0,
            0,-1,0,
            0,-1,0,
            0,-1,0,
                
            // top face
            0,1,0,
            0,1,0,
            0,1,0,
            0,1,0,
            0,1,0,
            0,1,0,
        }

    );
    
    
    
    public static readonly Mesh Triangle = new Mesh
    (
        vertices: new float[]
        {
            -1, -1, 0, 
            1, -1, 0, 
            0,  1, 0,
            
        },

        texCoords: new float[]
        {
            0,0, 1,0, 0.5f,1,
        },
            
        normals: new float[]
        {
            0,0,-1,
            0,0,-1,
            0,0,-1,
        }

    );    
    
    public static readonly Mesh Square = new Mesh
    (
        vertices: new float[]
        {
            -1, -1, 0, 
            1, -1, 0, 
            -1,  1, 0,
            
            1, -1, 0, 
            -1,  1, 0,
            1,  1, 0,
            
        },

        texCoords: new float[]
        {
            0,0, 1,0, 0,1,
            1,0, 0,1, 1,1,
        },
            
        normals: new float[]
        {
            0,0,-1,
            0,0,-1,
            0,0,-1,
            0,0,-1,
            0,0,-1,
            0,0,-1,
        }

    );
        
}