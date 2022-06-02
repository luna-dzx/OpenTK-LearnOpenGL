using Coordinate_Systems.Library;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Coordinate_Systems.Game;
public class Game1 : Library.Game
{
    private float[] vertices =
    {
        // positions          // colors           // texture coords
        0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 1.0f,   // top right
        0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f,   // bottom right
        -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 0.0f,   // bottom left
        -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f    // top left 
    };

    private int[] indices =
    {
        0, 1, 3, // first triangle
        1, 2, 3  // second triangle
    };
    
    private VertexArray vao;
    private ShaderProgram shaderProgram;

    private const string ShaderLocation = "../../../Game/Shaders/";

    private Texture texture0;
    private Texture texture1;

    protected override void Load()
    {
        GL.ClearColor(0.2f,0.3f,0.3f,1.0f);

        vao = new VertexArray();
        
        // I absolutely hate this way of doing it but I'm just proving it's possible in my engine
        vao.Add(0, vertices, BufferTarget.ArrayBuffer, 3,8,0);
        vao.Add(1, vertices, BufferTarget.ArrayBuffer, 3,8,3);
        vao.Add(2, vertices, BufferTarget.ArrayBuffer, 2,8,6);
        vao.StoreData(indices, BufferTarget.ElementArrayBuffer); vao.Use();
        
        
        shaderProgram = new ShaderProgram(ShaderLocation+"vertex.glsl",ShaderLocation+"fragment.glsl"); shaderProgram.Use();

        
        texture0 = new Texture(0,"../../../../../../0 Assets/container.jpg");
        texture1 = new Texture(1,"../../../../../../0 Assets/awesomeface.png");
        texture1.Mipmapping(TextureMinFilter.LinearMipmapLinear);

        texture0.LoadUniform(shaderProgram.GetHandle(),"texture0");
        texture1.LoadUniform(shaderProgram.GetHandle(),"texture1");

        shaderProgram.Use();
        
        shaderProgram.Uniform("mixValue");
        shaderProgram.Uniform("transform");

    }
    
    private Matrix4 transform = Matrix4.Identity;
    private float rotationAngle = 0f;
    private Vector3 translate = Vector3.Zero;
    

    protected override void KeyDown(KeyboardKeyEventArgs keyInfo)
    {
        if (keyInfo.Key == Keys.Escape) Window.Close();
    }

    private float mixValue = 0f;
    
    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Up)) mixValue = Math.Clamp(mixValue+(float)args.Time,0f,1f);
        if (keyboardState.IsKeyDown(Keys.Down)) mixValue = Math.Clamp(mixValue-(float)args.Time,0f,1f);

        if (keyboardState.IsKeyDown(Keys.Right)) rotationAngle -= 3f*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Left)) rotationAngle += 3f*(float)args.Time;

        if (keyboardState.IsKeyDown(Keys.W)) translate.Y += (float)args.Time;
        if (keyboardState.IsKeyDown(Keys.A)) translate.X -= (float)args.Time;
        if (keyboardState.IsKeyDown(Keys.S)) translate.Y -= (float)args.Time;
        if (keyboardState.IsKeyDown(Keys.D)) translate.X += (float)args.Time;
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        GL.Uniform1(shaderProgram.GetUniform("mixValue"),mixValue);
        
        transform = Matrix4.CreateRotationZ(rotationAngle) * Matrix4.CreateTranslation(translate);
        GL.UniformMatrix4(shaderProgram.GetUniform("transform"),false,ref transform);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.DrawElements(PrimitiveType.Triangles,indices.Length,DrawElementsType.UnsignedInt,0);

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        vao.Delete();
        shaderProgram.Delete();
        texture0.Delete();
        texture1.Delete();
    }
}
