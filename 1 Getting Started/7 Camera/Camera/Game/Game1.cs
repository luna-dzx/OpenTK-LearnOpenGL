using Camera.Library;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Camera.Game;
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

    private float[] texCoords = {
        0.0f, 0.0f,
        1.0f, 0.0f,
        1.0f, 1.0f,
        1.0f, 1.0f,
        0.0f, 1.0f,
        0.0f, 0.0f,
        0.0f, 0.0f,
        1.0f, 0.0f,
        1.0f, 1.0f,
        1.0f, 1.0f,
        0.0f, 1.0f,
        0.0f, 0.0f,
        1.0f, 0.0f,
        1.0f, 1.0f,
        0.0f, 1.0f,
        0.0f, 1.0f,
        0.0f, 0.0f,
        1.0f, 0.0f,
        1.0f, 0.0f,
        1.0f, 1.0f,
        0.0f, 1.0f,
        0.0f, 1.0f,
        0.0f, 0.0f,
        1.0f, 0.0f,
        0.0f, 1.0f,
        1.0f, 1.0f,
        1.0f, 0.0f,
        1.0f, 0.0f,
        0.0f, 0.0f,
        0.0f, 1.0f,
        0.0f, 1.0f,
        1.0f, 1.0f,
        1.0f, 0.0f,
        1.0f, 0.0f,
        0.0f, 0.0f,
        0.0f, 1.0f,
    };

    Vector3[] cubePositions = {
        new Vector3( 0.0f,  0.0f,  0.0f), 
        new Vector3( 2.0f,  5.0f, -15.0f), 
        new Vector3(-1.5f, -2.2f, -2.5f),  
        new Vector3(-3.8f, -2.0f, -12.3f),  
        new Vector3( 2.4f, -0.4f, -3.5f),  
        new Vector3(-1.7f,  3.0f, -7.5f),  
        new Vector3( 1.3f, -2.0f, -2.5f),  
        new Vector3( 1.5f,  2.0f, -2.5f), 
        new Vector3( 1.5f,  0.2f, -1.5f), 
        new Vector3(-1.3f,  1.0f, -1.5f)  
    };


    private Model cube;
    private ShaderProgram shaderProgram;

    private const string ShaderLocation = "../../../Game/Shaders/";

    private Texture texture0;
    private Texture texture1;

    private Library.Camera camera;

    protected override void Load()
    {
        GL.ClearColor(0.2f,0.3f,0.3f,1.0f);

        shaderProgram = new ShaderProgram(ShaderLocation+"vertex.glsl",ShaderLocation+"fragment.glsl"); shaderProgram.Use();

        texture0 = new Texture(0,"../../../../../../0 Assets/container.jpg");
        texture1 = new Texture(1,"../../../../../../0 Assets/awesomeface.png");
        texture1.Mipmapping(TextureMinFilter.LinearMipmapLinear);

        texture0.LoadUniform(shaderProgram.GetHandle(),"texture0");
        texture1.LoadUniform(shaderProgram.GetHandle(),"texture1");

        shaderProgram.Use();
    
        shaderProgram.Uniform("mixValue");
        shaderProgram.Uniform("model");
        shaderProgram.Uniform("view");
        shaderProgram.Uniform("proj");

        camera = new Library.Camera(
            shaderProgram.GetUniform("proj"),
            shaderProgram.GetUniform("view"),
            Window.Size
        );

        camera.Position = new Vector3(0, 0, 3);
        camera.Direction = new Vector3(0, 0, -1);
        
        cube = new Model(shaderProgram.GetUniform("model"));
        cube.LoadVertices(0,vertices);
        cube.LoadTexCoords(1,texCoords);

        Window.CursorState = CursorState.Grabbed;

    }

    private float mixValue = 0f;

    private Matrix3 rightTransform = Matrix3.CreateRotationY(MathHelper.PiOver2);

    private const float speed = 3f;
    

    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Up)) mixValue = Math.Clamp(mixValue+(float)args.Time,0f,1f);
        if (keyboardState.IsKeyDown(Keys.Down)) mixValue = Math.Clamp(mixValue-(float)args.Time,0f,1f);


        Vector3 directionFlat = camera.Direction;
        directionFlat.Y = 0;
        directionFlat.Normalize();
        
        var up = ((keyboardState.IsKeyDown(Keys.Space) ?1:0) - (keyboardState.IsKeyDown(Keys.LeftControl) ?1:0)) * (float)args.Time * speed;


        var input = Input.DirectionWASD(keyboardState) * speed * (float)args.Time;
        
        camera.Position += input.Z * directionFlat;
        camera.Position += input.X * (rightTransform * directionFlat);
        camera.Position += up * Vector3.UnitY;
    }


    protected override void Resize(ResizeEventArgs newWin) => camera.Resize(newWin.Size);

    private const float mouseSens = 1/20f;

    protected override void UpdateFrame(FrameEventArgs args)
    {
        GL.Uniform1(shaderProgram.GetUniform("mixValue"),mixValue);

        var mousePos = GetRelativeMouse();
    
        float yaw = mousePos.X*mouseSens -90;
        float pitch = mousePos.Y*mouseSens;


        camera.Direction.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
        camera.Direction.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
        camera.Direction.Y = (float)Math.Sin(MathHelper.DegreesToRadians(-pitch));
    
        camera.UpdateView();
    }

    private float rotationThingy = 0f;

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        float rotation = 0f;
        foreach (Vector3 pos in cubePositions)
        {

            cube.UpdateTransformation(
                pos,
                new Vector3(rotation,rotation*0.3f + rotationThingy,rotation*0.5f),
                new Vector3(2,1,1)
            );
            
            cube.Draw();
            rotation += MathHelper.Pi / 18f;
        }
    

        Window.SwapBuffers();

        rotationThingy += (float)args.Time;
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        cube.Delete();
        shaderProgram.Delete();
        texture0.Delete();
        texture1.Delete();
    }
}