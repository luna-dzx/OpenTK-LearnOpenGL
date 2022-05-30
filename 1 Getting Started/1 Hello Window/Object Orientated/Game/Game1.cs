using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;


namespace Object_Orientated.Game;
public class Game1 : Library.Game
{

    protected override void Load()
    {
        GL.ClearColor(0.2f,0.3f,0.3f,1.0f);
    }

    protected override void KeyDown(KeyboardKeyEventArgs keyInfo)
    {
        if (keyInfo.Key == Keys.Escape) Window.Close();
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        Window.SwapBuffers();
    }
    
}
