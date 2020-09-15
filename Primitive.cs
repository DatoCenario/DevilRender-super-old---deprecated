using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using Poly = System.Tuple<DevilRender.Vertex, DevilRender.Vertex, DevilRender.Vertex>;

namespace DevilRender
{
    public interface IObject3D
    {
        void Move(Vector3 v);
        void Rotate(float angle, Axis axis);
        Pivot Pivot { get; }
    }
    public abstract class Primitive : IObject3D
    {
        public FastBitmap Texture { get; protected set; }
        public Pivot Pivot { get; protected set; }
        public Vector3[] LocalVertices { get; protected set; }
        public Vector3[] GlobalVertices { get; protected set; }
        public Vector2[] TextureCoords { get; protected set; }
        public Vector3[] Normals { get; protected set; }
        public int[] Indexes { get; protected set; }
        public int[] TextureCoordsIndexes { get; protected set; }
        public int[] NormalIndexes { get; protected set; }

        public void Move(Vector3 v)
        {
            Pivot.Move(v);
            GlobalVertices = GlobalVertices.Select(i => i + v).ToArray();
        }

        public void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle , axis);
            GlobalVertices = LocalVertices.Select(v => Pivot.ToGlobalCoords(v)).ToArray();
        }
        public void RotateAt(Vector3 point, float angle, Axis axis)
        {
            Pivot.RotateAt(point , angle , axis);
            GlobalVertices = LocalVertices.Select(v => Pivot.ToGlobalCoords(v)).ToArray();
        }
        public IEnumerable<Poly> GetPolys()
        {
            for (int i = 0; i < Indexes.Length; i += 3)
            {
                Vector3 v1, v2, v3, vn1 = Vector3.Zero, vn2 = Vector3.Zero, vn3 = Vector3.Zero;
                Vector2 vt1 = Vector2.Zero, vt2 = Vector2.Zero, vt3 = Vector2.Zero;

                v1 = GlobalVertices[Indexes[i]];
                v2 = GlobalVertices[Indexes[i + 1]];
                v3 = GlobalVertices[Indexes[i + 2]];

                if (Texture != null)
                {
                    vt1 = TextureCoords[TextureCoordsIndexes[i]];
                    vt2 = TextureCoords[TextureCoordsIndexes[i + 1]];
                    vt3 = TextureCoords[TextureCoordsIndexes[i + 2]];
                }

                if (Normals.Length != 0)
                {
                    vn1 = Normals[NormalIndexes[i]];
                    vn2 = Normals[NormalIndexes[i + 1]];
                    vn3 = Normals[NormalIndexes[i + 2]];
                }

                var ver1 = new Vertex(v1, Color.Black, vt1 , vn1) { Texture  = Texture};
                var ver2 = new Vertex(v2 , Color.Black , vt2 , vn2) { Texture = Texture };
                var ver3 = new Vertex(v3 , Color.Black , vt3 , vn3) { Texture = Texture };

                yield return Tuple.Create(ver1 , ver2 , ver3);
            }
        }
        public void Scale(float k)
        {
            LocalVertices = LocalVertices.Select(v => v * k).ToArray();
            GlobalVertices = LocalVertices.Select(v => Pivot.ToGlobalCoords(v)).ToArray();
        }
    }
}
