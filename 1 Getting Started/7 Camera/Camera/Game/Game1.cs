using Camera.Library;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Camera.Game;
public class Game1 : Library.Game
{
    private float[] vertices =
    {
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

    private float[] texCoords =
    {
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

    private Matrix4 model;
    private Matrix4 view;
    private Matrix4 proj;
    
    private Vector2 startMousePos = Vector2.Zero;

    private Vector2i screenSize;
    
    protected override void Load()
    {
        GL.ClearColor(0.2f,0.3f,0.3f,1.0f);

        cube = new Model();
        cube.LoadVertices(0,vertices);
        cube.LoadTexCoords(1,texCoords);
        

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
        
        proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f),
            (float)Window.Size.X / Window.Size.Y, 0.1f, 100.0f);
        
        GL.UniformMatrix4(shaderProgram.GetUniform("proj"),false,ref proj);

        Window.CursorState = CursorState.Grabbed;

        startMousePos = Window.MousePosition;
        screenSize = Window.Size;

    }


    protected override void KeyDown(KeyboardKeyEventArgs keyInfo)
    {
        if (keyInfo.Key == Keys.Escape) Window.Close();
    }

    private float mixValue = 0f;

    private Vector3 playerPosition = new Vector3(0,0,3);
    private Vector3 playerDirection = new Vector3(0,0,-1);
    
    private Matrix3 rightTransform = Matrix3.CreateRotationY(MathHelper.PiOver2);

    private const float speed = 3f;
    private float fov = MathHelper.PiOver2;

    protected override void MouseScroll(MouseWheelEventArgs scroll)
    {
        fov = Math.Clamp(fov + 0.05f * scroll.OffsetY,0.1f,3.13f);
    }

    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Up)) mixValue = Math.Clamp(mixValue+(float)args.Time,0f,1f);
        if (keyboardState.IsKeyDown(Keys.Down)) mixValue = Math.Clamp(mixValue-(float)args.Time,0f,1f);


        Vector3 directionFlat = playerDirection;
        directionFlat.Y = 0;
        directionFlat.Normalize();
        
        var forward = ((keyboardState.IsKeyDown(Keys.W) ?1:0) - (keyboardState.IsKeyDown(Keys.S) ?1:0)) * (float)args.Time * speed;
        var right =  ((keyboardState.IsKeyDown(Keys.D) ?1:0) - (keyboardState.IsKeyDown(Keys.A) ?1:0)) * (float)args.Time * speed;
        var up = ((keyboardState.IsKeyDown(Keys.Space) ?1:0) - (keyboardState.IsKeyDown(Keys.LeftControl) ?1:0)) * (float)args.Time * speed;

        playerPosition += forward * directionFlat;
        playerPosition += right * (rightTransform * directionFlat);
        playerPosition += up * Vector3.UnitY;

    }
    

    protected override void Resize(ResizeEventArgs newSize)
    {
        screenSize = newSize.Size;
    }

    private float timePassed = 0f;

    private const float maxViewingAngle = 89.99f;
    private const float mouseSens = 1/20f;
    
    
    protected override void UpdateFrame(FrameEventArgs args)
    {
        GL.Uniform1(shaderProgram.GetUniform("mixValue"),mixValue);

        timePassed += (float)args.Time;
        
        
        float yaw = (Window.MousePosition.X - startMousePos.X)*mouseSens -90;
        float pitch = (Window.MousePosition.Y - startMousePos.Y)*mouseSens;

        if (pitch > maxViewingAngle) { pitch = maxViewingAngle; startMousePos.Y = Window.MousePosition.Y - maxViewingAngle/mouseSens; }
        if (pitch < -maxViewingAngle) { pitch = -maxViewingAngle; startMousePos.Y = Window.MousePosition.Y + maxViewingAngle/mouseSens; }
        
        
        playerDirection.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
        playerDirection.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
        playerDirection.Y = (float)Math.Sin(MathHelper.DegreesToRadians(-pitch));

        //model = Matrix4.CreateRotationY(timePassed) * Matrix4.CreateRotationX(timePassed*2f);
        view = Matrix4.LookAt(playerPosition, playerPosition + playerDirection, Vector3.UnitY);
        proj = Matrix4.CreatePerspectiveFieldOfView(fov,
            (float)screenSize.X / screenSize.Y, 0.1f, 100.0f);

        
        GL.UniformMatrix4(shaderProgram.GetUniform("view"),false,ref view);
        GL.UniformMatrix4(shaderProgram.GetUniform("proj"),false,ref proj);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        float rotation = 0f;
        foreach (Vector3 pos in cubePositions)
        {
            model = Matrix4.CreateRotationX(rotation) * Matrix4.CreateRotationY(rotation*0.3f) * Matrix4.CreateRotationZ(rotation*0.5f) * Matrix4.CreateTranslation(pos);
            GL.UniformMatrix4(shaderProgram.GetUniform("model"),false,ref model);
            cube.Draw();
            rotation += MathHelper.Pi / 18f;
        }
        

        Window.SwapBuffers();
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
