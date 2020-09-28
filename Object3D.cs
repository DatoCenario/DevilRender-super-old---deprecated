using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace DevilRender
{
    public abstract class Object3D
    {
        public delegate void RotateHandler(float angle , Axis axis);
        public delegate void MoveHandler(Vector3 v);
        public event RotateHandler OnRotate;
        public event MoveHandler OnMove;
        public Pivot Pivot { get; protected set; }
        public abstract void Move(Vector3 v);
        public abstract void Rotate(float angle, Axis axis);
        protected void OnRotateEvent(float angle, Axis axis)
        {
            OnRotate?.Invoke(angle , axis);
        }
        protected void OnMoveEvent(Vector3 v)
        {
            OnMove?.Invoke(v);
        }
    }
}
