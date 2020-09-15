using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using Poly = System.Tuple<System.Numerics.Vector3, System.Numerics.Vector3, System.Numerics.Vector3>;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

namespace DevilRender
{
    public enum Axis
    {
        X, Y, Z
    }
    static class VectorMath
    {
        public static float Cross(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }
        public static Vector3 Dot(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.Y * v2.Z - v1.Z * v2.Y, -(v1.X * v2.Z - v1.Z * v2.X), v1.X * v2.Y - v1.Y * v2.X);
        }
        public static float Angle(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Acos(Cross(v1, v2) / (v1.Length() * v2.Length()));
        }
        public static Vector3 Rotate(this Vector3 Vector3, float angle, Axis axis)
        {
            var rotation = axis == Axis.X ? Matrix4x4.CreateRotationX(angle) : axis == Axis.Y ? Matrix4x4.CreateRotationY(angle) : Matrix4x4.CreateRotationZ(angle);
            return Vector3.Transform(Vector3, rotation);
        }
        public static Vector3 Proection(Vector3 v1 , Vector3 v2)
        {
            return Cross(v1, v2) / Cross(v2, v2) * v2;
        }
        public static Vector3 Move(this Vector3 Vector3, float x, float y, float z)
        {
            return new Vector3(Vector3.X + x, Vector3.Y + y, Vector3.Z + z);
        }
        public static Vector3 Transform(this Vector3 Vector3, Matrix4x4 matrix4X4)
        {
            var v = new Vector4(Vector3.X, Vector3.Y, Vector3.Z, 1);
            var newVector = Vector4.Transform(v, matrix4X4);
            return new Vector3(newVector.X / newVector.W, newVector.Y / newVector.W, newVector.Z / newVector.W);
        }
        public static float[] GetLineCoefs(Vector3 v1, Vector3 v2)
        {
            var d = v2 - v1;
            if (d.X != 0 && d.Y != 0 && d.Z != 0)
            {
                var f = d.Y * d.Z; var s = -2 * d.X * d.Z; var t = d.Y * d.Z;
                return new float[] { f, s, t, -f * v1.X + s * v1.Y - t * v1.Z };
            }
            if (d.X == 0)
            {
                return new float[] { 0, d.Z, -d.Y, -d.Z * v1.Y + v1.Z * d.Y };
            }
            else if (d.Y == 0)
            {
                return new float[] { d.Z, -d.X, 0, -d.Z * v1.X + v1.Z * d.X };
            }
            else if (d.Z == 0)
            {
                return new float[] { d.Y, -d.X, 0, -d.Y * v1.X + v1.Y * d.X };
            }
            else
            {
                return new float[] { 0, 0, 0, 0 };
            }
        }
        public static bool LinesIntersect(Vector3 begin1, Vector3 end1, Vector3 begin2, Vector3 end2, out Vector3 intersectPoint)
        {
            intersectPoint = Vector3.Zero;
            var v1 = end1 - begin1;
            var v2 = end2 - begin2;
            if (Dot(v1, v2).Length() < 0.00001)
            {
                return false;
            }
            var coefs = GetLineCoefs(begin2, end2);
            var k = -(coefs[0] * begin1.X + coefs[1] * begin1.Y + coefs[2] * begin1.Z + coefs[3]) / (coefs[0] * v1.X + coefs[1] * v1.Y + coefs[2] * v1.Z);
            var intersect = begin1 + v1 * k;
            if (k >= 0 && k <= 1 && (Vector3.Normalize(v2) - Vector3.Normalize(intersect - begin2)).Length() < 0.00001)
            {
                intersectPoint = intersect;
                return true;
            }
            return false;
        }
        public static float[] GetSurfaceCoefs(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var v1 = p3 - p1;
            var v2 = p2 - p1;
            var dot = Dot(v1, v2);
            var d = -Cross(dot, p1);
            return new float[] { dot.X, dot.Y, dot.Z, d };
        }
        public static bool BelongsPoly(Vector3 p1 , Vector3 p2 , Vector3 p3, Vector3 point)
        {
            var v1 = p1 - point;
            var v2 = p2 - point;
            var v3 = p3 - point;
            var s1 = Dot(v1, v2).Length() + Dot(v2, v3).Length() + Dot(v3, v1).Length();
            var s2 = Dot(p3 - p1, p3 - p2).Length();
            return Math.Abs(s1 - s2) < 0.01;
        }
        public static bool AreIntersecting(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 start, Vector3 end, out Vector3 intersect)
        {
            var coefs = GetSurfaceCoefs(p1,p2,p3);
            var d = end - start;
            var scaleCoef = -(coefs[0] * start.X + coefs[1] * start.Y + coefs[2] * start.Z + coefs[3]) /
                (coefs[0] * d.X + coefs[1] * d.Y + coefs[2] * d.Z);
            var p = start + scaleCoef * d;
            if (scaleCoef >= 0 && scaleCoef <= 1 && BelongsPoly(p1,p2,p3, p))
            {
                intersect = p;
                return true;
            }
            intersect = Vector3.Zero;
            return false;
        }
    }
}
