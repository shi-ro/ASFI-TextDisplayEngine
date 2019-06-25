using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsfiEngine
{
    class SimpleSprite : GameObject
    {
        public SimpleSprite(int x, int y, Image image, bool camchild = false) : base(x, y, image, camchild) { }
        public override void Update() { }
    }
}
