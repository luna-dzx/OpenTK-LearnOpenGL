using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Depth_Testing.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model floor;
    Model cube1;
    Model cube2;
    
    int depthTestMode = (int)DepthFunction.Less;
    int visualiseDepthBuffer = 0; // 0 = no, 1 = yes, 2 = yes and linearize

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

    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyPressed(Keys.Backspace))
        {
            depthTestMode++;
            if (depthTestMode > 519) depthTestMode = 512;
            
            Console.WriteLine((DepthFunction)depthTestMode);
        }

        if (keyboardState.IsKeyPressed(Keys.Enter))
        {
            visualiseDepthBuffer++;
            if (visualiseDepthBuffer > 2) visualiseDepthBuffer = 0;
            shader.Uniform1("visualDepth", visualiseDepthBuffer);
        }

    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc((DepthFunction)depthTestMode);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.SetActive(ShaderType.FragmentShader,"cube");
        
        cube1.Transform(new Vector3(1f,0f,1f), Vector3.Zero, 1f);
        cube1.Draw();
        
        cube2.Transform(new Vector3(-2f,0f,0f), Vector3.Zero, 1f);
        cube2.Draw();
        
        
        shader.SetActive(ShaderType.FragmentShader,"floor");
        
        floor.Transform(new Vector3(0f,-1f,0f), new Vector3(MathF.PI/2f,0f,0f), 5f);
        floor.Draw();
        
        
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