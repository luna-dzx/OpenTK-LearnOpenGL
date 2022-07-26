using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Advanced_Data.Game;

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

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
        Window.UpdateFrame += args => player.Update(args,Window.KeyboardState,GetRelativeMouse());
    }
    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
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