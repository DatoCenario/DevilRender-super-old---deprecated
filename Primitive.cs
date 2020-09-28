using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

namespace DevilRender
{
    public class Primitive : Object3D
    {
        public Enviroment Enviroment { get; set; }
        public FastBitmap Texture { get; protected set; }
        public Pivot Pivot { get; protected set; }
        public Vector3[] LocalVertices { get; protected set; }
        public Vector3[] GlobalVertices { get; protected set; }
        public Vector2[] TextureCoords { get; protected set; }
        public Vector3[] Normals { get; protected set; }
        public int[] Indexes { get; protected set; }
        public int[] TextureCoordsIndexes { get; protected set; }
        public int[] NormalIndexes { get; protected set; }
        public IShader[] Shaders { get; set; }

        public Primitive(Vector3[] lv, Vector3[] gv, Vector3[] n, Vector2[] t, int[] i, int[] ni, int[] ti, IShader[] s, Pivot p)
        {
            Indexes = i;
            NormalIndexes = ni;
            TextureCoordsIndexes = ti;
            TextureCoords = t;
            Normals = n;
            LocalVertices = lv;
            GlobalVertices = gv;
            Shaders = s;
            Pivot = p;
        }
        public Primitive()
        {
            LocalVertices = new Vector3[0];
            GlobalVertices = new Vector3[0];
            TextureCoords = new Vector2[0];
            Normals = new Vector3[0];
            Indexes = new int[0];
            TextureCoordsIndexes = new int[0];
            NormalIndexes = new int[0];
        }

        public override void Move(Vector3 v)
        {
            Pivot.Move(v);
            GlobalVertices = GlobalVertices.Select(i => i + v).ToArray();
            OnMoveEvent(v);
            if (Enviroment != null)
            {
                Enviroment.OnChangeEvent();
            }
        }

        public override void Rotate(float angle, Axis axis)
        {
            Pivot.Rotate(angle, axis);
            GlobalVertices = LocalVertices.Select(v => Pivot.ToGlobalCoords(v)).ToArray();
            OnRotateEvent(angle, axis);
            if (Enviroment != null)
            {
                Enviroment.OnChangeEvent();
            }
        }
        public void RotateAt(Vector3 point, float angle, Axis axis)
        {
            Pivot.RotateAt(point, angle, axis);
            GlobalVertices = LocalVertices.Select(v => Pivot.ToGlobalCoords(v)).ToArray();
        }

        public Poly GetLocalPoly(Camera camera, int i)
        {
            Vector3 v1, v2, v3, vn1 = Vector3.Zero, vn2 = Vector3.Zero, vn3 = Vector3.Zero;
            Vector2 vt1 = Vector2.Zero, vt2 = Vector2.Zero, vt3 = Vector2.Zero;

            v1 = camera.Pivot.ToLocalCoords(GlobalVertices[Indexes[i]]);
            v2 = camera.Pivot.ToLocalCoords(GlobalVertices[Indexes[i + 1]]);
            v3 = camera.Pivot.ToLocalCoords(GlobalVertices[Indexes[i + 2]]);

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

            var ver1 = new Vertex(v1, new TGAColor(), vt1, vn1, this);
            var ver2 = new Vertex(v2, new TGAColor(), vt2, vn2, this);
            var ver3 = new Vertex(v3, new TGAColor(), vt3, vn3, this);

            return new Poly(ver1, ver2, ver3);
        }
        public IEnumerable<Poly> GetLocalPolys(Camera camera)
        {
            for (int i = 0; i < Indexes.Length; i += 3)
            {
                Vector3 v1, v2, v3, vn1 = Vector3.Zero, vn2 = Vector3.Zero, vn3 = Vector3.Zero;
                Vector2 vt1 = Vector2.Zero, vt2 = Vector2.Zero, vt3 = Vector2.Zero;

                v1 = camera.Pivot.ToLocalCoords(GlobalVertices[Indexes[i]]);
                v2 = camera.Pivot.ToLocalCoords(GlobalVertices[Indexes[i + 1]]);
                v3 = camera.Pivot.ToLocalCoords(GlobalVertices[Indexes[i + 2]]);

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

                var ver1 = new Vertex(v1, new TGAColor(), vt1, vn1, this);
                var ver2 = new Vertex(v2, new TGAColor(), vt2, vn2, this);
                var ver3 = new Vertex(v3, new TGAColor(), vt3, vn3, this);

                yield return new Poly(ver1, ver2, ver3);
            }
        }
        public void Scale(float k)
        {
            LocalVertices = LocalVertices.Select(v => v * k).ToArray();
            GlobalVertices = LocalVertices.Select(v => Pivot.ToGlobalCoords(v)).ToArray();
        }

        public Primitive Clone()
        {
            return new Primitive(LocalVertices.ToArray(),
                GlobalVertices.ToArray(),
                Normals.ToArray(),
                TextureCoords.ToArray(),
                Indexes.ToArray(),
                NormalIndexes.ToArray(),
                TextureCoordsIndexes.ToArray(),
                Shaders.ToArray(),
                Pivot.Clone());
        }
    }
}
