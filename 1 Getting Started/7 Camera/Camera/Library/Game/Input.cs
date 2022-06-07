using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace Camera.Library;
public class Input
{
    private static readonly float OneOverRoot2 = 1f/(float)Math.Sqrt(2);
        
    public static Vector3 DirectionWASD(KeyboardState keyboardState)
    {
        int forwards = (keyboardState.IsKeyDown(Keys.W)) ? 1 : 0;
        int backwards = (keyboardState.IsKeyDown(Keys.S)) ? 1 : 0;
        int left = (keyboardState.IsKeyDown(Keys.A)) ? 1 : 0;
        int right = (keyboardState.IsKeyDown(Keys.D)) ? 1 : 0;

        float mult = 1f;
        // diagonal
        if (Math.Abs(right - left) + Math.Abs(forwards - backwards) > 1) mult = OneOverRoot2;
        return new Vector3(right-left,0,forwards-backwards) * mult;
    }
}