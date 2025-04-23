using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderItemViewModel
{
    public class VMSoItem
    {
        public long SoItemId { get; set; }

        public long SoOrderId { get; set; }

        public string ItemName { get; set; } = null!;

        public int Quantity { get; set; }

        public double Price { get; set; }
    }
}
