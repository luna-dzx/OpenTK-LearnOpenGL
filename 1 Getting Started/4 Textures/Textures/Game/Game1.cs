using Textures.Library;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;

namespace Textures.Game;
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
        
        
        using (var stream = File.OpenRead("../../../../../../0 Assets/container.jpg"))
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
        
        GL.BindTexture(TextureTarget.Texture2D,texture);


    }

    protected override void KeyDown(KeyboardKeyEventArgs keyInfo)
    {
        if (keyInfo.Key == Keys.Escape) Window.Close();
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
