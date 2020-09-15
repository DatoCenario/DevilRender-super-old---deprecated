using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Render
{
    public class Enviroment
    {
        public List<Primitive> Primitives { get; private set; }
        pubilc delegate void ChangeHandler;
        public event ChangeHandler OnChange;

        public Enviroment(params Primitive[] primitives)
        {
            Primitives = primitives.ToList();
        }

        public void AddPrimitive(Primitive primitive)
        {
            Primitives.Add(primitive);
            OnChange?.Invoke();
        }
    }
}
