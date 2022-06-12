using Library;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Colours.Game;
public class Game1 : Library.Game
{
    private float[] vertices = {
        // positions
        -0.5f, -0.5f, -0.5f, 
        0.5f, -0.5f, -0.5f, 
        0.5f,  0.5f, -0.5f, 
        0.5f,  0.5f, -0.5f, 
        -0.5f,  0.5f, -0.5f, 
        -0.5f, -0.5f, -0.5f, 

        -0.5f, -0.5f,  0.5f, 
        0.5f, -0.5f,  0.5f, 
        0.5f,  0.5f,  0.5f, 
        0.5f,  0.5f,  0.5f, 
        -0.5f,  0.5f,  0.5f, 
        -0.5f, -0.5f,  0.5f, 

        -0.5f,  0.5f,  0.5f, 
        -0.5f,  0.5f, -0.5f, 
        -0.5f, -0.5f, -0.5f, 
        -0.5f, -0.5f, -0.5f, 
        -0.5f, -0.5f,  0.5f, 
        -0.5f,  0.5f,  0.5f, 

        0.5f,  0.5f,  0.5f, 
        0.5f,  0.5f, -0.5f, 
        0.5f, -0.5f, -0.5f, 
        0.5f, -0.5f, -0.5f, 
        0.5f, -0.5f,  0.5f, 
        0.5f,  0.5f,  0.5f, 

        -0.5f, -0.5f, -0.5f, 
        0.5f, -0.5f, -0.5f, 
        0.5f, -0.5f,  0.5f, 
        0.5f, -0.5f,  0.5f, 
        -0.5f, -0.5f,  0.5f, 
        -0.5f, -0.5f, -0.5f, 

        -0.5f,  0.5f, -0.5f, 
        0.5f,  0.5f, -0.5f, 
        0.5f,  0.5f,  0.5f, 
        0.5f,  0.5f,  0.5f, 
        -0.5f,  0.5f,  0.5f, 
        -0.5f,  0.5f, -0.5f, 
    };

    private Model cube;
    private ShaderProgram shaderProgram;

    private const string ShaderLocation = "../../../Game/Shaders/";

    private FirstPersonPlayer player;
    
    Vector3 lightPos = new Vector3(1.2f, 1.0f, 2.0f);

    protected override void Load()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        shaderProgram = new ShaderProgram(ShaderLocation+"vertex.glsl",ShaderLocation+"fragment.glsl");
        shaderProgram.Use();
        
        shaderProgram.Uniform("model");
        shaderProgram.Uniform("view");
        shaderProgram.Uniform("proj");
        shaderProgram.Uniform("colour");

        player = new FirstPersonPlayer(
            shaderProgram.GetUniform("proj"),
            shaderProgram.GetUniform("view"),
            Window.Size
        );
        

        player.Position = new Vector3(0, 0, 3);
        player.Direction = new Vector3(0, 0, -1);
        
        cube = new Model(shaderProgram.GetUniform("model"));
        cube.LoadVertices(0,vertices);

        Window.CursorState = CursorState.Grabbed;

    }
    
    protected override void Resize(ResizeEventArgs newWin) => player.Camera.Resize(newWin.Size);
    protected override void UpdateFrame(FrameEventArgs args) => player.Update(args,Window.KeyboardState,GetRelativeMouse());
    
    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        GL.Uniform3(shaderProgram.GetUniform("colour"),1.0f,1.0f,1.0f);
        cube.UpdateTransformation(lightPos,Vector3.Zero,new Vector3(0.6f,0.6f,0.6f));
        cube.Draw();
        
        GL.Uniform3(shaderProgram.GetUniform("colour"),1.0f, 0.5f, 0.31f);
        cube.UpdateTransformation(Vector3.Zero, Vector3.Zero,new Vector3(0.6f,0.6f,0.6f));
        cube.Draw();

        Window.SwapBuffers();

    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        cube.Delete();
        shaderProgram.Delete();
    }
}