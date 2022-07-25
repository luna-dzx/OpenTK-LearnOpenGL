using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace Cubemaps.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model backpack;
    Model cube;
    
    Texture cubeMap;

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

        
        cubeMap = Texture.LoadCubeMap(0,"../../../../../../0 Assets/skybox/",".jpg");
        
        shader.UniformTexture("cubemap", cubeMap);
        
        backpack = Model.FromFile(
            "../../../../../../0 Assets/backpack/", "backpack.obj",
            out var textures,
            shader.DefaultModel,
            Array.Empty<TextureType>()
        );
        
        cube = new Model(PresetMesh.Cube, shader.DefaultModel);
        
        cube.ResetTransform();
        backpack.ResetTransform();
        
        GL.Enable(EnableCap.DepthTest);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(args,Window.KeyboardState,GetRelativeMouse());
        shader.Uniform3("cameraPos", player.Camera.Position);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.DepthFunc(DepthFunction.Lequal);
        
        cubeMap.Use();

        GL.Enable(EnableCap.CullFace);
        shader.SetActive(ShaderType.FragmentShader,"scene");
        shader.SetActive(ShaderType.VertexShader,"scene");
        //texture.Use();
        backpack.Draw();
        
        GL.Disable(EnableCap.CullFace);
        shader.SetActive(ShaderType.FragmentShader,"cubemap");
        shader.SetActive(ShaderType.VertexShader,"cubemap");
        
        cube.Draw();
        
        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        backpack.Delete();
        cube.Delete();
        
        shader.Delete();
    }
}