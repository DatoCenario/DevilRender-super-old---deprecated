using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Poly = System.Tuple<System.Numerics.Vector3, System.Numerics.Vector3, System.Numerics.Vector3>;
using Poly2D = System.Tuple<System.Numerics.Vector2, System.Numerics.Vector2, System.Numerics.Vector2>;
using System.Runtime.InteropServices;
using System.Drawing;
using DevilRender;

namespace DevilRender
{
    public class Camera : Object3D
    {
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public Pivot Pivot { get; private set; }
        public float ScreenDist { get; private set; }
        public float ObserveRange { get; private set; }
        public float Scale => ScreenWidth / (float)(2 * ScreenDist * Math.Tan(ObserveRange / 2));
        public Camera(Vector3 center, float screenDist, float observeRange, int screenWidth, int screenHeight)
        {
            Pivot = Pivot.BasePivot(center);
            ScreenDist = screenDist;
            ObserveRange = observeRange;
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
        }
        public override void Move(Vector3 v)
        {
            Pivot.Move(v);
            OnMoveEvent(v);
        }
        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);
            OnRotateEvent(angle , axis);
        }
        public Vector2 ScreenProection(Vector3 local)
        {
            var delta = ScreenDist / local.Z * Scale;
            var proection = new Vector2(local.X * delta, local.Y * delta);
            var inScreenBasis = proection + new Vector2(ScreenWidth / 2, -ScreenHeight / 2);
            return new Vector2(inScreenBasis.X, -inScreenBasis.Y);
        }
        public bool InObserve(Vector3 local)
        {
            if (local.Z <= ScreenDist)
            {
                return false;
            }
            var angle = VectorMath.Angle(Vector3.UnitZ, local);
            if (Math.Abs(angle) > ObserveRange / 2)
            {
                return false;
            }
            return true;
        }
    }
}


