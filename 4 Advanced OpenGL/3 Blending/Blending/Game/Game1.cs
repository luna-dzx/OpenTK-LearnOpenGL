using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace Stencil_Testing.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model square;

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

        Texture floorTexture = new Texture("../../../../../../0 Assets/metal.png",0)
            .Wrapping(TextureParameterName.TextureWrapS,TextureWrapMode.ClampToEdge);
        Texture grassTexture = new Texture("../../../../../../0 Assets/grass.png",1)
            .Wrapping(TextureParameterName.TextureWrapS,TextureWrapMode.ClampToEdge);
        Texture glassTexture = new Texture("../../../../../../0 Assets/window.png",2)
            .Wrapping(TextureParameterName.TextureWrapS,TextureWrapMode.ClampToEdge);

        shader.UniformTexture("floorTexture", floorTexture);
        shader.UniformTexture("grassTexture", grassTexture);
        shader.UniformTexture("glassTexture", glassTexture);
        
        square = new Model(PresetMesh.Square, shader.DefaultModel);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
        Window.UpdateFrame += args => player.Update(args,Window.KeyboardState,GetRelativeMouse());
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(true);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.SetActive(ShaderType.FragmentShader,"floor");
        
        square.Transform(new Vector3(0f,-1f,0f), new Vector3(MathF.PI/2f,0f,0f), 5f);
        square.Draw();
        
        
        
        shader.SetActive(ShaderType.FragmentShader,"grass");
        
        square.Transform(new Vector3(1f,0f,1f), Vector3.Zero, 1f);
        square.Draw();
        
        // an improper fix to prevent manually ordering since in the future I'll switch to order independent transparency
        // works ok ish for this example
        GL.DepthMask(false);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha,BlendingFactor.OneMinusSrcAlpha);
        
        shader.SetActive(ShaderType.FragmentShader,"glass");
        
        square.Transform(new Vector3(-0.5f,0f,-1f), Vector3.Zero, 1f);
        shader.Uniform3("filterColour", 0f, 0f, 1f);
        square.Draw();
        
        square.Transform(new Vector3(-1f,0f,0f), Vector3.Zero, 1f);
        shader.Uniform3("filterColour", 1f, 0f, 0f);
        square.Draw();
        
        square.Transform(new Vector3(0f,0f,2f), Vector3.Zero, 1f);
        shader.Uniform3("filterColour", 0f, 1f, 0f);
        square.Draw();
        

        
        
        


        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        square.Delete();


        shader.Delete();
    }
}