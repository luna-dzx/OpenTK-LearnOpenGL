using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Colours.Game;

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
            .EnableAutoProjection();

        player = new FirstPersonPlayer(shader.DefaultProjection, shader.DefaultView, Window.Size)
            .SetPosition(new Vector3(0, 0, 3))
            .SetDirection(new Vector3(0, 0, -1));
            
        cube = new Model(shader.DefaultModel)
            .LoadVertices(0,PresetMesh.Cube.Vertices);
            
        shader.Uniform3("colour", 1.0f, 0.5f, 0.31f);

        Window.CursorState = CursorState.Grabbed;
    }
        
    protected override void Resize(ResizeEventArgs newWin) => player.Camera.Resize(newWin.Size);
    protected override void UpdateFrame(FrameEventArgs args) => player.Update(args,Window.KeyboardState,GetRelativeMouse());

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            
        shader.SetActive(ShaderType.FragmentShader, "lightShader");
        cube.Transform(lightPos, Vector3.Zero, 0.3f);
        cube.Draw();

        shader.SetActive(ShaderType.FragmentShader, "shader");
        cube.Transform(Vector3.Zero, Vector3.Zero, 0.3f);
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