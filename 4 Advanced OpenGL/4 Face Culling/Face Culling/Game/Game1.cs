using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace Face_Culling.Game;

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

        Texture texture = new Texture("../../../../../../0 Assets/marble.jpg",0);
        

        shader.UniformTexture("texture0", texture);
        cube = new Model(PresetMesh.Cube, shader.DefaultModel);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
        Window.UpdateFrame += args => player.Update(args,Window.KeyboardState,GetRelativeMouse());
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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