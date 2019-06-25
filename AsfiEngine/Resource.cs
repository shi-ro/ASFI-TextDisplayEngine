using AsfiEngine.Extra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsfiEngine
{
    public class Resource : MapObject
    {
        public int Ammount { get; private set; }
        public ResourceType Type = ResourceType.None;
        public Resource(string name, Image picture, ResourceType type, int ammount) : base(name,picture)
        {
            Type = type;
            Ammount = ammount;
        }
    }
}
