using System;
using System.Collections.Generic;
using System.Text;

namespace DevilRender
{
    public struct TGAColor
    {
        public byte a;
        public byte r;
        public byte g;
        public byte b;

        public TGAColor(byte a, byte r, byte g, byte b)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }
}
