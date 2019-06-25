using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsfiEngine
{
    public class InventoryItem
    {
        public int Ammount = 1;
        public MenuObject Item { get; set; }
        public InventoryItem(MenuObject item)
        {
            Item = item;
        }
    }
}
