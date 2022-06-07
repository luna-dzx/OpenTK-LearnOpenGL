using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Camera.Library;

public class Camera
{
    // projection matrix vars
    private float aspect;
    private float fov;
    private float depthNear;
    private float depthFar;

    // camera vars
    public Vector3 Position;
    public Vector3 Direction;
    private readonly Vector3 up = Vector3.UnitY;

    // OpenGL
    private int uProj;
    private int uView;

    private Matrix4 proj;
    private Matrix4 view;

    public Camera(int projectionBinding, int viewBinding, float aspectRatio, float fieldOfView = MathHelper.PiOver3,float clipNear = 0.1f, float clipFar = 100f)
    {
        aspect = aspectRatio;
        fov = fieldOfView;
        depthNear = clipNear;
        depthFar = clipFar;
    
        uProj = projectionBinding;
        uView = viewBinding;

        UpdateProjection();
        UpdateView();
    }

    public Camera(int projectionBinding, int viewBinding, Vector2i windowSize, float fieldOfView = MathHelper.PiOver3, float clipNear = 0.1f, float clipFar = 100f):this(projectionBinding,viewBinding,(float)windowSize.X/windowSize.Y,fieldOfView,clipNear,clipFar) { }
    


    public void UpdateProjection()
    {
        proj = Matrix4.CreatePerspectiveFieldOfView(fov, aspect, depthNear, depthFar);
        GL.UniformMatrix4(uProj,false,ref proj);
    }

    public void Resize(float newAspect)
    {
        aspect = newAspect;
        UpdateProjection();
    }
    public void Resize(Vector2i newSize)
    {
        Resize((float)newSize.X / newSize.Y);
    }

    public void SetFov(float fieldOfView)
    {
        fov = fieldOfView;
        UpdateProjection();
    }

    public void SetDepth(float near, float far)
    {
        depthNear = near;
        depthFar = far;
        UpdateProjection();
    }

    public void UpdateView(bool flipCamera = false)
    {
        view = Matrix4.LookAt(Position, Position + Direction, ((flipCamera)?-1:1) * up);
        GL.UniformMatrix4(uView,false,ref view);
    }

}