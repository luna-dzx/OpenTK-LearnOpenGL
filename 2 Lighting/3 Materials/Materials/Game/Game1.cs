using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Materials.Game;

public class Game1 : Library.Game
{
    private const string ShaderLocation = "../../../Game/Shaders/";
    private ShaderProgram shader;

    private FirstPersonPlayer player;
    private Model cube;

    Vector3 lightPos = new Vector3(1.2f, 1.0f, 2.0f);

    protected override void Load()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        shader = new ShaderProgram(ShaderLocation + "vertex.glsl", ShaderLocation + "fragment.glsl")
            .EnableAutoProjection()
            .Uniform3("objectColour", 1.0f, 0.5f, 0.31f)
            .Uniform3("lightColour", 1.0f, 1.0f, 1.0f)

            .Uniform3("material.ambient", 0.0f,0.05f,0.0f)
            .Uniform3("material.diffuse", 0.4f,0.5f,0.4f)
            .Uniform3("material.specular", 0.04f,0.7f,0.04f)
            .Uniform1("material.shininess", 10f)

            .Uniform3("light.position", lightPos)
            .Uniform3("light.ambient", 1.0f, 1.0f, 1.0f)
            .Uniform3("light.diffuse", 1.0f, 1.0f, 1.0f)
            .Uniform3("light.specular", 1.0f, 1.0f, 1.0f);

        player = new FirstPersonPlayer(shader.DefaultProjection, shader.DefaultView, Window.Size)
            .SetPosition(new Vector3(0, 0, 3))
            .SetDirection(new Vector3(0, 0, -1));

        cube = new Model(PresetMesh.Cube, shader.DefaultModel);
    }
        
    protected override void Resize(ResizeEventArgs newWin) => player.Camera.Resize(newWin.Size);

    private float angle;
    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(args,Window.KeyboardState,GetRelativeMouse());

        lightPos = 4 * (Matrix3.CreateRotationZ(0.05f*angle) * Matrix3.CreateRotationY(0.5f*angle) * Vector3.UnitZ);
        angle += (float)args.Time;
        
        shader.Uniform3("lightPos", lightPos);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            
        shader.SetActive(ShaderType.FragmentShader, "lightShader");
        cube.Transform(lightPos, Vector3.Zero, 0.1f);
        cube.Draw();

        shader.SetActive(ShaderType.FragmentShader, "shader");
        cube.ResetTransform();
        cube.Draw();
            

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        cube.Delete();
        shader.Delete();
    }
    
}