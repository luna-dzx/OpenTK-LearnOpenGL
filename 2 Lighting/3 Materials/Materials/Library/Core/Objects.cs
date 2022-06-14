using OpenTK.Mathematics;

namespace Library;

public static class Objects
{
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


    public class Material
    {
        public Vector3 Ambient;
        public Vector3 Diffuse;
        public Vector3 Specular;
        public float Shininess;


        public Material(){}
            
        public Material(
            float ambientR, float ambientG, float ambientB,
            float diffuseR, float diffuseG, float diffuseB,
            float specularR, float specularG, float specularB,
            float shininess
        )
        {
            SetAmbient(ambientR, ambientG, ambientB);
            SetDiffuse(diffuseR, diffuseG, diffuseB);
            SetSpecular(specularR, specularG, specularB);
            SetShininess(shininess);
        }

        public Material SetAmbient(Vector3 ambient) { Ambient = ambient; return this; }
        public Material SetAmbient(float r, float g, float b) { Ambient = new Vector3(r,g,b); return this; }
        public Material SetDiffuse(Vector3 diffuse) { Diffuse = diffuse; return this; }
        public Material SetDiffuse(float r, float g, float b) { Diffuse = new Vector3(r,g,b); return this; }
        public Material SetSpecular(Vector3 specular) { Specular = specular; return this; }
        public Material SetSpecular(float r, float g, float b) { Specular = new Vector3(r,g,b); return this; }
        public Material SetShininess(float shininess) { Shininess = shininess; return this; }
            
    }

    public class Light
    {
        public Vector3 Position;
        public Vector3 Ambient = Vector3.One;
        public Vector3 Diffuse = Vector3.One;
        public Vector3 Specular = Vector3.One;

        public Light SetPosition(Vector3 position) { Position = position; return this; }
        public Light SetPosition(float x, float y, float z) { Position = new Vector3(x,y,z); return this; }

        public Light UpdatePosition(ref ShaderProgram shaderProgram, string name)
        {
            shaderProgram.Uniform3(name + ".position", Position);
            return this;
        }

        public Light SetAmbient(Vector3 ambient) { Ambient = ambient; return this; }
        public Light SetAmbient(float r, float g, float b) { Ambient = new Vector3(r,g,b); return this; }
        public Light SetDiffuse(Vector3 diffuse) { Diffuse = diffuse; return this; }
        public Light SetDiffuse(float r, float g, float b) { Diffuse = new Vector3(r,g,b); return this; }
        public Light SetSpecular(Vector3 specular) { Specular = specular; return this; }
        public Light SetSpecular(float r, float g, float b) { Specular = new Vector3(r,g,b); return this; }
    }
    
}