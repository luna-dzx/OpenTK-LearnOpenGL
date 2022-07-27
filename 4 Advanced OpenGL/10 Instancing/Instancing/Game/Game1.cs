using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Instancing.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model asteroid;
    Model planet;
    Model cube;

    Texture asteroidTexture;
    Texture planetTexture;
    Texture cubeMap;

    Objects.Light light;
    Objects.Material material;

    protected override void Load()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        shader = new ShaderProgram(
            ShaderLocation + "vertex.glsl", 
            ShaderLocation + "fragment.glsl",
            true);


        player = new FirstPersonPlayer(Window.Size, speed: 20f)
            .SetPosition(new Vector3(1f, 22f, 66f)*2f)
            .SetDirection(new Vector3(0, 0, -1));
        player.Camera.SetDepth(0.1f,500f);
        player.UpdateProjection(shader);


        light = new Objects.Light()
            .SetAmbient(0.1f, 0.1f, 0.1f)
            .SetPosition(-100f, 100f, 0f);
            //.SunMode().SetDirection(new Vector3(1,-1,1));
        material = new Objects.Material()
            .SetAmbient(0.1f)
            .SetDiffuse(1.2f)
            .SetSpecular(2f)
            .SetShininess(32f);

        shader.UniformLight("light",light);

        var asteroidMesh = Model.FromFile(
            "../../../../../../0 Assets/rock/", "rock.obj",
            out _,
            Array.Empty<TextureType>()
        );

        asteroid = new Model();

        Matrix4[] modelMatrices = new Matrix4[50000];
        Random r = new Random(1);
        float dist = 130f;
        float posVar = 50f;
        float vertPosVar = 0.2f;
        float scaleVar = 0.2f;
        float baseScale = 0.3f;
        
        for (int i = 0; i < 50000; i++)
        {
            var pos = (new Vector3(r.NextSingle()*2 -1, 0f, r.NextSingle()*2 -1).Normalized()+ Vector3.UnitY * (r.NextSingle() * vertPosVar - vertPosVar/2f)).Normalized() 
                      * (dist + (r.NextSingle() * posVar - posVar/2f))
                      ;
            var rot = new Vector3(r.NextSingle() * MathF.PI*2, r.NextSingle() * MathF.PI*2, r.NextSingle() * MathF.PI*2);
            var scale = baseScale + (r.NextSingle() * scaleVar - scaleVar/2f);
            modelMatrices[i] = Maths.CreateTransformation(pos, rot, new Vector3(scale,scale,scale));
        }

        asteroid.StoreData(modelMatrices, BufferTarget.ArrayBuffer);
        GL.BindVertexArray(asteroid.GetHandle());

        int vec4size = 4*sizeof(float);
        
        GL.EnableVertexAttribArray(3);
        GL.VertexAttribPointer(3,4,VertexAttribPointerType.Float,false,4*vec4size,IntPtr.Zero);        
        GL.EnableVertexAttribArray(4);
        GL.VertexAttribPointer(4,4,VertexAttribPointerType.Float,false,4*vec4size,(IntPtr) vec4size);        
        GL.EnableVertexAttribArray(5);
        GL.VertexAttribPointer(5,4,VertexAttribPointerType.Float,false,4*vec4size,(IntPtr) (2 * vec4size));        
        GL.EnableVertexAttribArray(6);
        GL.VertexAttribPointer(6,4,VertexAttribPointerType.Float,false,4*vec4size,(IntPtr) (3 * vec4size));
        
        GL.VertexAttribDivisor(3,1);
        GL.VertexAttribDivisor(4,1);
        GL.VertexAttribDivisor(5,1);
        GL.VertexAttribDivisor(6,1);
        
        asteroid.Add(0, asteroidMesh.Vertices);
        asteroid.Add(1, asteroidMesh.TexCoords, BufferTarget.ArrayBuffer, 2, 2);
        asteroid.Add(2, asteroidMesh.Normals);
        

        asteroid.StoreData(asteroidMesh.Indices, BufferTarget.ElementArrayBuffer);

        asteroid.SetMesh(asteroidMesh);

        
        planet = new Model(Model.FromFile(
                "../../../../../../0 Assets/planet/", "planet.obj",
                out _,
                Array.Empty<TextureType>()
            ))
        .Scale(5f)
            ;

        cube = new Model(PresetMesh.Cube);


        asteroidTexture = new Texture("../../../../../../0 Assets/rock/rock.png",0);
        planetTexture = new Texture("../../../../../../0 Assets/planet/mars.png",0);
        cubeMap = Texture.LoadCubeMap("../../../../../../0 Assets/SpaceboxCollection/Spacebox4/",".png", 1);
        
        shader.UniformMaterial("material",material,asteroidTexture);
        shader.UniformTexture("cubemap",cubeMap);
        

        GL.Enable(EnableCap.DepthTest);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(shader,newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(shader, args, Window.KeyboardState, GetRelativeMouse());
        shader.Uniform3("cameraPos", player.Camera.Position);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.Use();
        
        GL.Disable(EnableCap.CullFace);
        GL.DepthMask(false);
        
        shader.SetActive(ShaderType.FragmentShader, "cubemap");
        shader.SetActive(ShaderType.VertexShader, "cubemap");
        cube.UpdateTransform(shader);
        cube.Draw();
        
        
        
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);
        
        shader.SetActive(ShaderType.FragmentShader, "scene");
        
        shader.SetActive(ShaderType.VertexShader, "asteroid");
        asteroidTexture.Use();
        asteroid.UpdateTransform(shader);
        asteroid.Draw(50000);
        
        
        shader.SetActive(ShaderType.VertexShader, "planet");
        planetTexture.Use();
        planet.UpdateTransform(shader);
        planet.Draw();
        

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        asteroid.Delete();
        planet.Delete();
        cube.Delete();
        
        shader.Delete();
    }
}