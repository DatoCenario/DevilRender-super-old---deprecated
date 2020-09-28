using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using System.Numerics;
using System.Globalization;
using System.Drawing;

namespace DevilRender
{
    public class Model : Primitive
    {
        public Model(Vector3[] vertices,  Vector2[] texCoords , Vector3[] normals , int[] indexes , int[] texCoordsIndexes , int[] normalIndexes , FastBitmap texture)
        {
            Pivot = Pivot.BasePivot(Vector3.Zero);
            LocalVertices = vertices;
            GlobalVertices = vertices.Select(v => Pivot.ToGlobalCoords(v)).ToArray();
            TextureCoords = texCoords;
            Normals = normals;
            Indexes = indexes;
            TextureCoordsIndexes = texCoordsIndexes;
            NormalIndexes = normalIndexes;
            Texture = texture;
        }
    }
    static class ObjParser
    {
        public static Primitive FromObjFile(string filePath , string texturePath)
        {
            var texture = texturePath == null ? null : FastBitmap.FromBitmap((Bitmap)Image.FromFile(texturePath));
            var vertices = new List<Vector3>();
            var indexes = new List<int>();
            var textureCoords = new List<Vector2>();
            var textureCoordsIndexes = new List<int>();
            var normals = new List<Vector3>();
            var normalIndexes = new List<int>();
            var reader = new StreamReader(filePath);
            while (!reader.EndOfStream)
            {
                var currentLine = reader.ReadLine();
                if (currentLine != "")
                {
                    if (currentLine[0] == 'v')
                    {
                        ReadVertex(currentLine , vertices , textureCoords , normals);
                    }
                    else if (currentLine[0] == 'f')
                    {
                        ReadPolygon(currentLine , indexes , textureCoordsIndexes , normalIndexes);
                    }
                }
            }
            var model = new Model(
                vertices.ToArray() , 
                textureCoords.ToArray() ,
                normals.ToArray() ,
                indexes.ToArray() , 
                textureCoordsIndexes.ToArray() , 
                normalIndexes.ToArray() , 
                texture);
            return model;
        }

        static void ReadVertex(string line , List<Vector3> vertices , List<Vector2> texCoords , List<Vector3> normals)
        {
            var nums = line.Split(' ').Skip(1)
                .Where(n => n != "")
                .Select(n => float.Parse(n, CultureInfo.InvariantCulture))
                .ToArray();

            switch (line[1])
            {
                case 't':
                    texCoords.Add(new Vector2(nums[0], nums[1]));
                    break;
                case 'n':
                    normals.Add(Vector3.Normalize(new Vector3(nums[0], nums[1], nums[2])));
                    break;
                default:
                    vertices.Add(new Vector3(nums[0], nums[1], nums[2]));
                    break;
            }
        }

        static void ReadPolygon(string line , List<int> indexes , List<int> textureCoordsIndexes , List<int> normalIndexes)
        {
            var ind = line.Split(' ').Skip(1)
                .Where(l => l != "")
                .Select(l => l.Split('/').Select(n => n == "" ? 0 : int.Parse(n)).ToArray())
                .ToArray();

            for (int i = 1; i < ind.Length - 1; i++)
            {
                indexes.Add(ind[0][0]-1);
                indexes.Add(ind[i][0] - 1);
                indexes.Add(ind[i + 1][0] - 1);

                if (ind[0].Length > 1)
                {
                    textureCoordsIndexes.Add(ind[0][1] - 1);
                    textureCoordsIndexes.Add(ind[i][1] - 1);
                    textureCoordsIndexes.Add(ind[i + 1][1] - 1);
                }
                if (ind[0].Length > 2)
                {

                    normalIndexes.Add(ind[0][2] - 1);
                    normalIndexes.Add(ind[i][2] - 1);
                    normalIndexes.Add(ind[i + 1][2] - 1);
                }
            }
        }
    }
}
