using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderItemViewModel
{
    public class VMSoOrder
    {
        public long SoOrderId { get; set; }

        public string OrderNo { get; set; } = null!;

        public DateTime OrderDate { get; set; }

        public int ComCustomerId { get; set; }

        public string? ComCustomerName { get; set; }

        public string Address { get; set; } = null!;

        public List<VMSoItem>? Items { get; set; }
    }
}
