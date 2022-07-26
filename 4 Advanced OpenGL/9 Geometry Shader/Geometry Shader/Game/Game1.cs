using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Geometry_Shader.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;
    ShaderProgram normalShader;

    FirstPersonPlayer player;
    Model backpack;

    protected override void Load()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        shader = new ShaderProgram(
            ShaderLocation + "vertex.glsl", 
            ShaderLocation + "fragment.glsl",
            true);

        
        normalShader = new ShaderProgram()
            .LoadShader(ShaderLocation + "normals/vertex.glsl", ShaderType.VertexShader)
            .LoadShader(ShaderLocation + "normals/geometry.glsl", ShaderType.GeometryShader)
            .LoadShader(ShaderLocation + "normals/fragment.glsl", ShaderType.FragmentShader)
            .Compile()
            .EnableAutoProjection()
            ;
        
        shader.Use();

        // todo: do this a better away to allow for multiple shaders
        player = new FirstPersonPlayer(shader.DefaultProjection, shader.DefaultView, Window.Size)
            .SetPosition(new Vector3(0, 0, 3))
            .SetDirection(new Vector3(0, 0, -1));

        
        normalShader.Use();
        Matrix4 proj = player.Camera.GetProjMatrix();
        GL.UniformMatrix4(normalShader.DefaultProjection,false,ref proj);

        shader.Use();

        backpack = Model.FromFile(
            "../../../../../../0 Assets/backpack/", "backpack.obj",
            out var textures,
            shader.DefaultModel,
            new [] { TextureType.Diffuse}
        );

        Console.WriteLine(textures[TextureType.Diffuse].Count);
        
        Texture texture = textures[TextureType.Diffuse][0].Use();
        shader.UniformTexture("texture0", texture);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.ProgramPointSize);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        shader.Use();
        player.Update(args,Window.KeyboardState,GetRelativeMouse());
        
        
        normalShader.Use();
        Matrix4 view = player.Camera.GetViewMatrix();
        GL.UniformMatrix4(normalShader.DefaultView,false,ref view);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.Use();

        backpack.Draw();
        
        
        normalShader.Use();
        
        Matrix4 model = backpack.GetTransform();
        GL.UniformMatrix4(normalShader.DefaultModel,false,ref model);
        
        backpack.Draw();

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        backpack.Delete();
        
        shader.Delete();
        normalShader.Delete();
    }
}