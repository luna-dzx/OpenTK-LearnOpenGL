using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Stencil_Testing.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model floor;
    Model cube1;
    Model cube2;

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

        Texture floorTexture = new Texture("../../../../../../0 Assets/metal.png",0);
        Texture cubeTexture = new Texture("../../../../../../0 Assets/marble.jpg",1);

        shader.UniformTexture("floorTexture", floorTexture);
        shader.UniformTexture("cubeTexture", cubeTexture);

        floor = new Model(PresetMesh.Square, shader.DefaultModel);
        cube1 = new Model(PresetMesh.Cube, shader.DefaultModel);
        cube2 = new Model(PresetMesh.Cube, shader.DefaultModel);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
        Window.UpdateFrame += args => player.Update(args,Window.KeyboardState,GetRelativeMouse());
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.StencilTest);
        GL.StencilOp(StencilOp.Keep,StencilOp.Keep,StencilOp.Replace);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        
        GL.StencilMask(0x00);

        shader.SetActive(ShaderType.FragmentShader,"floor");
        
        floor.Transform(new Vector3(0f,-1.01f,0f), new Vector3(MathF.PI/2f,0f,0f), 5f);
        floor.Draw();
        
        GL.StencilFunc(StencilFunction.Always,1,0xFF);
        GL.StencilMask(0xFF);
        

        shader.SetActive(ShaderType.FragmentShader,"cube");
        
        cube1.Transform(new Vector3(1f,0f,1f), Vector3.Zero, 1f);
        cube1.Draw();
        
        cube2.Transform(new Vector3(-2f,0f,0f), Vector3.Zero, 1f);
        cube2.Draw();
        
        
        GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
        GL.StencilMask(0x00);
        GL.Disable(EnableCap.DepthTest);
        
        shader.SetActive(ShaderType.FragmentShader,"flatColour");
        
        cube1.Transform(new Vector3(1f,0f,1f), Vector3.Zero, 1.1f);
        cube1.Draw();
        
        cube2.Transform(new Vector3(-2f,0f,0f), Vector3.Zero, 1.1f);
        cube2.Draw();
        
        GL.StencilMask(0xFF);
        GL.StencilFunc(StencilFunction.Always,1,0xFF);
        
        
        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        floor.Delete();
        cube1.Delete();
        cube2.Delete();
        
        shader.Delete();
    }
}