using System;
using System.Collections.Generic;

namespace OrderItemModel;

public partial class ViewSoItem
{
    public long SoItemId { get; set; }

    public long SoOrderId { get; set; }

    public string ItemName { get; set; } = null!;

    public int Quantity { get; set; }

    public double Price { get; set; }
}
