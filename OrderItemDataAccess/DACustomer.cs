using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OrderItemModel;
using OrderItemViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OrderItemDataAccess
{
    public class DACustomer
    {
        private OlDevContext db;

        public DACustomer(OlDevContext _db)
        {
            db = _db;
        }

        public VMResponse<List<VMComCustomer>?> GetAllCustomers()
        {
            VMResponse<List<VMComCustomer>?> response = new VMResponse<List<VMComCustomer>?>();

            try
            {
                response.Data = (
                    from cust in db.ComCustomers
                    select new VMComCustomer
                    {
                        ComCustomerId = cust.ComCustomerId,
                        CustomerName = cust.CustomerName
                    }
                    ).ToList();

                if (response.Data != null && response.Data.Count > 0 )
                {
                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = $"{HttpStatusCode.OK} - Successfully fetched all customer data!";
                    response.TotalData = response.Data.Count;
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NoContent;
                    response.Message = $"{HttpStatusCode.NoContent} - No customer data was Found!";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From DACustomer.GetAllCustomers: {ex.Message}";
            }

            return response;
        }
    }
}
