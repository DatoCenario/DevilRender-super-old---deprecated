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
        public float ZoneRadius { get; }
        public Dictionary<Vector3 , List<Primitive>> Zones { get; private set; }
        public List<Primitive> Primitives { get; private set; }
        public Enviroment(int zoneRaduis, params Primitive[] primitives)
        {
            ZoneRadius = zoneRaduis;
            Primitives = primitives.ToList();
            Zones = new Dictionary<Vector3, List<Primitive>>();
            foreach (var p in primitives)
            {
                var zone = GetZoneKey(p.Pivot.Center);
                if (Zones.ContainsKey(zone))
                {
                    Zones[zone].Add(p);
                }
                else
                {
                    Zones[zone] = new List<Primitive>() { p };
                }
            }
        }
        public List<Primitive> GetPrimitivesInZone(Vector3 zone)
        {
            if (Zones.ContainsKey(zone))
            {
                return Zones[zone];
            }
            else
            {
                return new List<Primitive>();
            }
        }
        public Vector3 GetZoneKey(Vector3 v)
        {
            return new Vector3(
                (v.X / ZoneRadius) * ZoneRadius,
                (v.Y / ZoneRadius) * ZoneRadius,
                (v.Z / ZoneRadius) * ZoneRadius
                );
        }
    }
}
