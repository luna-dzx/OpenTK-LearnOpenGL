using Library;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TextRendering.Game;

public class Game1 : Library.Game
{
    StateHandler glState;

    protected override void Initialize()
    {
        glState = new StateHandler();
        glState.ClearColor = Color4.Teal;
    }

    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState k) { }


    protected override void RenderFrame(FrameEventArgs args)
    {
        glState.Clear();
        Window.SwapBuffers();
    }

    protected override void Unload() { }
}