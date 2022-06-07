using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Camera.Library;

/// <summary>
/// Simplification of movement and camera handling - dependent on Camera class
/// </summary>
public abstract class Player
{
    public Vector3 Position;
    public Camera Camera;
    
    
    public Player(int projectionBinding, int viewBinding, Vector2i windowSize, float fov = MathHelper.PiOver3)
    {
        Camera = new Camera(projectionBinding, viewBinding, windowSize, fov);
    }    
    
    public Player(int projectionBinding, int viewBinding, float aspectRatio, float fov = MathHelper.PiOver3)
    {
        Camera = new Camera(projectionBinding, viewBinding, aspectRatio, fov);
    }    
    
    public Player(ref Camera camera)
    {
        Camera = camera;
    }
    
    protected void Update(bool flipCamera = false, bool renderingPaused = false)
    {
        if (!renderingPaused) Camera.UpdateView(flipCamera);
    }

}




public class FirstPersonPlayer : Player
{
    public Vector3 Velocity;
    public float Sensitivity;
    public float Speed;

    private Vector3 unitGravity = -Vector3.UnitY;
    public void SetGravity(Vector3 direction) => unitGravity = direction;

    public FirstPersonPlayer(int projectionBinding, int viewBinding, Vector2i windowSize, float fov = MathHelper.PiOver3, float sensitivity = 1/20f, float speed = 5f)
        : base(projectionBinding,viewBinding,windowSize,fov)
    {
        Sensitivity = sensitivity;
        Speed = speed;
    }

    private readonly Matrix3 rightTransform = Matrix3.CreateRotationY(MathHelper.PiOver2);

    private Vector2 lastMousePos;
    private float yaw;
    private float pitch;

    private bool capPitch = true;
    
    public void Update(FrameEventArgs args, KeyboardState keyboardState, Vector2 relativeMousePos)
    {
        var input = Input.DirectionWASD(keyboardState) * Speed * (float)args.Time;
        yaw += (relativeMousePos.X - lastMousePos.X) * Sensitivity;
        pitch += (relativeMousePos.Y - lastMousePos.Y) * Sensitivity;

        yaw %= 360;
        pitch %= 360;

        // 90 degrees gives gimbal locking so lock to 89
        if (capPitch) pitch = Math.Clamp(pitch, -89f, 89f);

        Camera.Direction = Matrix3.CreateRotationY(MathHelper.DegreesToRadians(yaw)) * Matrix3.CreateRotationX(MathHelper.DegreesToRadians(pitch)) * -Vector3.UnitZ;
        
        Vector3 up = ((keyboardState.IsKeyDown(Keys.Space) ?1:0) - (keyboardState.IsKeyDown(Keys.LeftControl) ?1:0)) * Speed * (float)args.Time * Vector3.UnitY;
        
        Vector3 directionFlat = Camera.Direction;
        directionFlat.Y = 0;
        directionFlat.Normalize();

        Velocity = input.Z * directionFlat + input.X * (rightTransform * directionFlat) + up;
        
        Position += Velocity;
        Camera.Position = Position;

        base.Update((Math.Abs(pitch)+90)%360 >= 180);
        lastMousePos = relativeMousePos;
    }


    public Vector3 Direction
    {
        get => Camera.Direction;
        set => Camera.Direction=value;
    }
    
}



