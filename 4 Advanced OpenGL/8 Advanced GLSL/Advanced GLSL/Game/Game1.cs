using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Advanced_GLSL.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model cube;

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

        cube = new Model(PresetMesh.Cube, shader.DefaultModel);

        Texture texture = new Texture("../../../../../../0 Assets/container.jpg", 0).Use();
        shader.UniformTexture("texture0", texture);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.ProgramPointSize);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
        Window.UpdateFrame += args => player.Update(args,Window.KeyboardState,GetRelativeMouse());
    }
    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        cube.Transform(new Vector3(-2,0,0),Vector3.Zero, 1f);

        shader.SetActive(ShaderType.VertexShader, "points");
        shader.SetActive(ShaderType.FragmentShader, "points");
        cube.Draw(PrimitiveType.Points);
        
        shader.SetActive(ShaderType.VertexShader, "main");
        shader.SetActive(ShaderType.FragmentShader, "main");
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