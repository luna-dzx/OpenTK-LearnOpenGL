using Object_Oriented.Library;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;

namespace Object_Oriented.Game;
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

    protected override void Load()
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        
        GL.ClearColor(0.2f,0.3f,0.3f,1.0f);

        vao = new VertexArray();
        
        // I absolutely hate this way of doing it but I'm just proving it's possible in my engine
        vao.Add(0, vertices, BufferTarget.ArrayBuffer, 3,8,0);
        vao.Add(1, vertices, BufferTarget.ArrayBuffer, 3,8,3);
        vao.Add(2, vertices, BufferTarget.ArrayBuffer, 2,8,6);
        vao.StoreData(indices, BufferTarget.ElementArrayBuffer); vao.Use();
        
        
        shaderProgram = new ShaderProgram(ShaderLocation+"vertex.glsl",ShaderLocation+"fragment.glsl"); shaderProgram.Use();
        

        int texture;
        texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D,texture);
        
        #region Texture Wrapping
        // S corresponds with the X component of the 2D texture
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        // T corresponds with the Y component of the 2D texture
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        #endregion

        #region Texture Filtering
        // Filtering for minifying (render smaller than texture)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        // Filtering for magnifying (render bigger than texture)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        #endregion

        #region Mipmapping
        // Mipmapping for minifying (render smaller than texture)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        // (no need to set a mipmapping option for magnifying)
        #endregion

        using (var stream = File.OpenRead("../../../../../../0 Assets/container.jpg"))
        {
            ImageResult image = ImageResult.FromStream(stream,ColorComponents.RedGreenBlueAlpha);
            GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,image.Width,image.Height,0,PixelFormat.Rgba,PixelType.UnsignedByte,image.Data);
        }
        
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        int texture1;
        texture1 = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D,texture1);

        using (var stream = File.OpenRead("../../../../../../0 Assets/awesomeface.png"))
        {
            ImageResult image = ImageResult.FromStream(stream,ColorComponents.RedGreenBlueAlpha);
            GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,image.Width,image.Height,0,PixelFormat.Rgba,PixelType.UnsignedByte,image.Data);
        }
        
        #region Texture Wrapping
        // S corresponds with the X component of the 2D texture
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        // T corresponds with the Y component of the 2D texture
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        #endregion

        #region Texture Filtering
        // Filtering for minifying (render smaller than texture)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        // Filtering for magnifying (render bigger than texture)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        #endregion

        #region Mipmapping
        // Mipmapping for minifying (render smaller than texture)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        // (no need to set a mipmapping option for magnifying)
        #endregion
        
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        shaderProgram.Use();
        GL.Uniform1(GL.GetUniformLocation(shaderProgram.GetHandle(),"texture0"),0);
        GL.Uniform1(GL.GetUniformLocation(shaderProgram.GetHandle(),"texture1"),1);

        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D,texture1);
        
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D,texture);
        
        shaderProgram.Use();
        shaderProgram.Uniform("mixValue");

    }

    protected override void KeyDown(KeyboardKeyEventArgs keyInfo)
    {
        if (keyInfo.Key == Keys.Escape) Window.Close();
    }

    private float mixValue = 0f;
    
    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Up)) mixValue = Math.Clamp(mixValue+(float)args.Time,0f,1f);
        if (keyboardState.IsKeyDown(Keys.Down)) mixValue = Math.Clamp(mixValue-(float)args.Time,0f,1f);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        GL.Uniform1(shaderProgram.GetUniform("mixValue"),mixValue);
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
    }
}
