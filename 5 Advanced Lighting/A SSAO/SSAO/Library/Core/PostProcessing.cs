using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Library;

public class PostProcessing
{
    public static void Draw() => GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

    public void DrawFbo() { ReadTexture(); Draw(); }

    [Flags]
    public enum PostProcessShader
    {
        GaussianBlur = 1,
        // go up from here in powers of 2 so it's possible to use | on this
    }

    private Dictionary<PostProcessShader,ShaderProgram> shaderPrograms;

    public ShaderProgram BlitShader;

    public FrameBuffer WriteFbo;
    public FrameBuffer ReadFbo;


    private string GenerateBlitShader(int numAttachments)
    {
        string output =
            "#version 330 core\n";

        for (int i = 0; i < numAttachments; i++)
        {
            output += "layout (location = "+i+") out vec4 fragColour"+i+";\n";
        }
        
        for (int i = 0; i < numAttachments; i++)
        {
            output += "uniform sampler2D texture"+i+";\n";
        }

        output +=
            "in vec2 texCoords;\n" +

            "void main()\n" +
            "{\n";
        
        for (int i = 0; i < numAttachments; i++)
        {
            output += "fragColour"+i+" = texture(texture"+i+",texCoords);\n";
        }
        
        output += "}\n";
        
        Console.WriteLine(output);

       return output;
    }
    
    
    private static readonly DrawBuffersEnum[] DefaultAttachments = { DrawBuffersEnum.ColorAttachment0 };
    
    private const string LibraryShaderPath = "../../../Library/Shaders/";
    
    public PostProcessing(PostProcessShader postProcessEffects, Vector2i frameBufferSize, PixelInternalFormat internalFormat = PixelInternalFormat.Rgba8, DrawBuffersEnum[]? colourAttachments = null)
    {
        colourAttachments ??= DefaultAttachments;
        
        WriteFbo = new FrameBuffer(frameBufferSize,internalFormat: internalFormat,numColourAttachments:colourAttachments.Length);
        ReadFbo = new FrameBuffer(frameBufferSize,internalFormat: internalFormat,numColourAttachments:colourAttachments.Length);
        
        
        BlitShader = new ShaderProgram()
            .LoadPostProcessVertex()
            .LoadShaderText(GenerateBlitShader(colourAttachments.Length),ShaderType.FragmentShader)
            .Compile();

        for (int i = 0; i < colourAttachments.Length; i++)
        {
            WriteFbo.UniformTexture((int)BlitShader, "texture"+i, i);
        }
        
        ReadFbo.SetDrawBuffers(colourAttachments);
        WriteFbo.SetDrawBuffers(colourAttachments);

        shaderPrograms = new Dictionary<PostProcessShader, ShaderProgram>();

        // loop through post processing effects
        foreach (var postProcessShader in Enum.GetValues(typeof(PostProcessShader)).Cast<PostProcessShader>())
        {
            // if "postProcessEffects" has this effect
            if ((postProcessEffects & postProcessShader) != 0)
            {
                // add to dictionary of effects
                shaderPrograms[postProcessShader] = new ShaderProgram
                (
                    LibraryShaderPath + "PostProcessing/gaussianFragment.glsl"
                );
                
                WriteFbo.UniformTexture((int)shaderPrograms[postProcessShader], "texture0", 1);
            }
        }

    }
    
    
    public PostProcessing BlitFbo()
    {
        BlitShader.Use();
        ReadFbo.WriteMode();
        
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        WriteFbo.UseTexture();
        Draw();
        
        ReadFbo.ReadMode();

        return this;
    }
    

    private enum BlurDirection
    {
        Horizontal,
        Vertical
    }
    
    private void GaussianBlurStep(DrawBuffersEnum[] colourAttachments, BlurDirection direction)
    {
        shaderPrograms[PostProcessShader.GaussianBlur].Uniform1("blurDirection", (int)direction);
        WriteFbo.WriteMode();
        WriteFbo.SetDrawBuffers(colourAttachments);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        ReadFbo.UseTexture();
        Draw();
        
        WriteFbo.ReadMode();

        BlitFbo();
    }
    
    
    public PostProcessing RenderEffect(PostProcessShader effect, DrawBuffersEnum[]? colourAttachments = null)
    {
        colourAttachments ??= DefaultAttachments; 
        
        if ((effect & PostProcessShader.GaussianBlur) != 0)
        {
            shaderPrograms[PostProcessShader.GaussianBlur].Use();

            GaussianBlurStep(colourAttachments, BlurDirection.Horizontal);
            GaussianBlurStep(colourAttachments, BlurDirection.Vertical);

        }
        
        
        


        return this;
    }


    public PostProcessing ReadTexture()
    {
        ReadFbo.UseTexture();
        return this;
    }


    public PostProcessing StartSceneRender(DrawBuffersEnum[]? colourAttachments = null)
    {
        if (colourAttachments != null) { WriteFbo.SetDrawBuffers(colourAttachments); }
        WriteFbo.WriteMode();
        return this;
    }
    public PostProcessing EndSceneRender(bool depthMask = false)
    {
        WriteFbo.ReadMode();
        // no need to measure depth since nothing beyond this point actually renders in 3d
        GL.DepthMask(depthMask);
        BlitFbo();
        return this;
    }

    public PostProcessing UniformTexture(ShaderProgram program, string name, int binding)
    {
        WriteFbo.UniformTexture((int)program, name, binding);
        return this;
    }
    
    public PostProcessing UniformTextures(ShaderProgram program, string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            WriteFbo.UniformTexture((int)program, names[i], i);
        }
        
        return this;
    }
    
    

    public void Delete()
    {
        ReadFbo.Delete();
        WriteFbo.Delete();
        
        foreach (var program in shaderPrograms.Values)
        {
            program.Delete();
        }
        
        BlitShader.Delete();
        
    }
}