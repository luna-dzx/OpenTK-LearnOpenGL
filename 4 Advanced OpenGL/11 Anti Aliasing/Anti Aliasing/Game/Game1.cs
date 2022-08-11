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
    Model cube;
    Model quad;

    FrameBuffer fbo;

    protected override void Load()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        shader = new ShaderProgram(
            ShaderLocation + "vertex.glsl", 
            ShaderLocation + "fragment.glsl",
            true);
        
        player = new FirstPersonPlayer(Window.Size)
            .SetPosition(new Vector3(0, 0, 3))
            .SetDirection(new Vector3(0, 0, 1));
        player.UpdateProjection(shader);

        cube = new Model(PresetMesh.Cube);
        quad = new Model(PresetMesh.Square);

        fbo = new FrameBuffer(Window.Size, TextureTarget.Texture2DMultisample);
        fbo.UseTexture();

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(shader,newWin.Size);
        Window.UpdateFrame += args => player.Update(shader, args, Window.KeyboardState, GetRelativeMouse());
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        shader.Use();
        
        fbo.WriteMode();
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.DepthTest);
        
        shader.SetActive(ShaderType.VertexShader, "scene");
        shader.SetActive(ShaderType.FragmentShader, "scene");

        cube.ResetTransform(); cube.UpdateTransform(shader);
        shader.Uniform3("cubeColour", 0f, 1f, 0f);
        cube.Draw();
        
        cube.Transform(new Vector3(1,1,-3),Vector3.Zero, 1f); cube.UpdateTransform(shader);
        shader.Uniform3("cubeColour", 0f, 0f, 1f);
        cube.Draw();
        
        
        fbo.ReadMode();

        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.Disable(EnableCap.DepthTest);

        shader.SetActive(ShaderType.VertexShader, "fbo");
        shader.SetActive(ShaderType.FragmentShader, "fbo");
        quad.Draw();
        

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        cube.Delete();
        quad.Delete();
        fbo.Delete();
        
        shader.Delete();
    }
}