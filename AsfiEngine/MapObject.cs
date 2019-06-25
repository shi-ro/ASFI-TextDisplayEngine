using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsfiEngine
{
    public class MapObject
    {
        public string Name { get; private set; }
        public Image Picture { get; private set; }

        public MapObject(string name, Image image)
        {
            Name = name;
            Picture = image;
        }
    }
}
