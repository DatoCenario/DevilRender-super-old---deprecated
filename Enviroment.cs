using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DevilRender
{
    public class Enviroment
    {
        public delegate void ChangeHandler();
        public event ChangeHandler OnChange;
        List<Primitive> Primitives;
        public IEnumerable<Primitive> GetPrimitives()
        {
            foreach (var p in Primitives) yield return p;
        }
        public Enviroment(int zoneRaduis, params Primitive[] primitives)
        {
            Primitives = primitives.ToList();
        }
        public void AddPrimitive(Primitive primitive)
        {
            Primitives.Add(primitive);
            primitive.Enviroment = this;
        }
        public void OnChangeEvent()
        {
            OnChange?.Invoke();
        }
    }
}
