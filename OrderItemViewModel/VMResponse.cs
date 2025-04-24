using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OrderItemViewModel
{
    public class VMResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public long? TotalData { get; set; }
        public long? TotalItem { get; set; }
        public double? TotalAmount { get; set; }

        public VMResponse()
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Message = string.Empty;
            Data = default(T);
            TotalData = 0;
            TotalItem = 0;
            TotalAmount = 0;
        }
    }
}
