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
    Model quad;

    Texture texture;

    FrameBuffer frameBuffer;

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

        texture = new Texture("../../../../../../0 Assets/awesomeface.png",0);
        

        shader.UniformTexture("texture0", texture);
        cube = new Model(PresetMesh.Cube, shader.DefaultModel);
        quad = new Model(PresetMesh.Square, shader.DefaultModel);
        
        frameBuffer = new FrameBuffer(PixelFormat.Rgb,Window.Size);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
        Window.UpdateFrame += args => player.Update(args,Window.KeyboardState,GetRelativeMouse());
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.CullFace);
        
        frameBuffer.WriteMode();
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.DepthTest);

        texture.Use();
        
        shader.SetActive(ShaderType.VertexShader,"scene");
        shader.SetActive(ShaderType.FragmentShader,"scene");
        cube.ResetTransform();
        cube.Draw();    
        

        frameBuffer.ReadMode();
        
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.Disable(EnableCap.DepthTest);
        
        frameBuffer.UseTexture();
        
        shader.SetActive(ShaderType.VertexShader,"quad");
        shader.SetActive(ShaderType.FragmentShader,"quad");
        quad.ResetTransform();
        quad.Draw();
        

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        cube.Delete();
        frameBuffer.Delete();
        shader.Delete();
    }
}