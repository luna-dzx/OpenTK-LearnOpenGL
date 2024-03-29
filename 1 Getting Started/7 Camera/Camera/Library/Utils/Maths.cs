﻿using OpenTK.Mathematics;

namespace Camera.Library;

public class Maths
{
    public static Matrix4 CreateTransformation(Vector3 translate, Vector3 rotate, Vector3 scale)
    {
        Vector3 sin = new Vector3(MathF.Sin(rotate.X), MathF.Sin(rotate.Y), MathF.Sin(rotate.Z));
        Vector3 cos = new Vector3(MathF.Cos(rotate.X), MathF.Cos(rotate.Y), MathF.Cos(rotate.Z));

        Matrix4 result;
        
        result.Row0.X = scale.X * cos.Y * cos.Z;
        result.Row0.Y = scale.X * cos.Y * sin.Z;
        result.Row0.Z = scale.X * -sin.Y;
        result.Row0.W = 0;
        result.Row1.X = scale.Y * (sin.X * sin.Y * cos.Z + cos.X * -sin.Z);
        result.Row1.Y = scale.Y * (sin.X * sin.Y * sin.Z + cos.X * cos.Z);
        result.Row1.Z = scale.Y * sin.X * cos.Y;
        result.Row1.W = 0;
        result.Row2.X = scale.Z * (cos.X * sin.Y * cos.Z + -sin.X * -sin.Z);
        result.Row2.Y = scale.Z * (cos.X * sin.Y * sin.Z + -sin.X * cos.Z);
        result.Row2.Z = scale.Z * cos.X * cos.Y;
        result.Row2.W = 0;
        result.Row3.X = translate.X;
        result.Row3.Y = translate.Y;
        result.Row3.Z = translate.Z;
        result.Row3.W = 1;

        return result;

    }
}