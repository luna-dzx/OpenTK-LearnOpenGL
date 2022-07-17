using System.Runtime.InteropServices;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Multiple_Lights.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model cube;

    Objects.Light light;
    Objects.Light spotLight;
    Objects.Light sun;
    Objects.Material material;
        
    Texture texture;
    Texture textureSpecular;

    Matrix4[] cubeTransforms;

    protected override void Load()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        shader = new ShaderProgram(
            ShaderLocation + "vertex.glsl", 
            ShaderLocation + "fragment.glsl",
            true);

        player = new FirstPersonPlayer(shader.DefaultProjection, shader.DefaultView, Window.Size)
            .SetPosition(new Vector3(0, 0, 3))
            .SetDirection(new Vector3(0, 0, -1));

        texture = new Texture("../../../../../../0 Assets/container2.png",0);
        textureSpecular = new Texture("../../../../../../0 Assets/container2_specular.png",1);

        cube = new Model(PresetMesh.Cube, shader.DefaultModel);

        spotLight = new Objects.Light()
            .SetPosition(0,0,6f)
            .SetDirection(-Vector3.UnitZ)
            .SetAmbient(0,0,0)
            .SetDiffuse(1,1,1)
            .SetSpecular(1,1,1)
            .SetAttenuation(1,0.01f,0.01f)
            .SpotlightMode(MathHelper.DegreesToRadians(12.5f),MathHelper.DegreesToRadians(15f));

        sun = new Objects.Light()
            .SetDirection(-2,-1,-1)
            .SetAmbient(0.3f,0.3f,0.3f)
            .SetDiffuse(0.55f,0.55f,0.55f)
            .SetSpecular(0.5f,0.5f,0.5f)
            .SunMode();

        light = new Objects.Light()
            .SetPosition(-2f, 3f, -4f);

        material = PresetMaterial.Silver;

        shader
            .Uniform3("objectColour", 1.0f, 0.5f, 0.31f)
            .UniformLight("lights[0]", sun)
            .UniformLight("lights[1]", spotLight)
            .UniformLight("lights[2]", light)
            .UniformMaterial("material", material, texture, textureSpecular);


        cubeTransforms = new []
        {
            Maths.TranslateMatrix( 0.0f,  0.0f,  0.0f),
            Maths.TranslateMatrix( 2.0f,  5.0f, -15.0f),
            Maths.TranslateMatrix(-1.5f, -2.2f, -2.5f),
            Maths.TranslateMatrix(-3.8f, -2.0f, -12.3f),
            Maths.TranslateMatrix( 2.4f, -0.4f, -3.5f),
            Maths.TranslateMatrix(-1.7f,  3.0f, -7.5f),
            Maths.TranslateMatrix( 1.3f, -2.0f, -2.5f),
            Maths.TranslateMatrix( 1.5f,  2.0f, -2.5f),
            Maths.TranslateMatrix( 1.6f,  0.2f, -1.5f),
            Maths.TranslateMatrix(-1.3f,  1.0f, -1.5f)
        };


    }
        
    protected override void Resize(ResizeEventArgs newWin) => player.Camera.Resize(newWin.Size);

    private float angle;
    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(args,Window.KeyboardState,GetRelativeMouse());

        //light.Position = 4 * (Matrix3.CreateRotationZ(0.05f*angle) * Matrix3.CreateRotationY(0.5f*angle) * Vector3.UnitZ);
        //angle += (float)args.Time;


        shader.Uniform3("cameraPos", player.Camera.Position);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            
        shader.SetActive(ShaderType.FragmentShader, "lightShader");
        cube.Transform(spotLight.Position, Vector3.Zero, 0.2f);
        cube.Draw();
        cube.Transform(light.Position, Vector3.Zero, 0.2f);
        cube.Draw();

        shader.SetActive(ShaderType.FragmentShader, "shader");
        //cube.ResetTransform();
        foreach (var transform in cubeTransforms)
        {
            cube.UpdateTransformation(transform);
            cube.Draw();
        }
        
        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        cube.Delete();
        texture.Delete();
        textureSpecular.Delete();
        shader.Delete();
    }
    
}