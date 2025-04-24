using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using OrderItemModel;
using OrderItemViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OrderItemDataAccess
{
    public class DAItem
    {
        private OlDevContext db;

        public DAItem(OlDevContext _db)
        {
            db = _db;
        }

        public VMResponse<List<VMSoItem>?> GetTemporaryItemsByOrderId(long orderId)
        {
            VMResponse<List<VMSoItem>?> response = new VMResponse<List<VMSoItem>?>();

            try
            {
                response.Data = (
                    from item in db.SoItems
                    where item.SoOrderId == orderId * -1
                    select new VMSoItem
                    {
                        SoItemId = item.SoItemId,
                        SoOrderId = orderId,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }
                    ).ToList();

                if (response.Data != null && response.Data.Count > 0)
                {
                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = $"{HttpStatusCode.OK} - Successfully fetched the data";
                    response.TotalData = response.Data.Count;
                }
                else
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Message = $"{HttpStatusCode.BadRequest} - No order data was Found!";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From DAItem.GetTemporaryItemsByOrderId: {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMSoItem> AddTemporaryItems(VMSoItem data)
        {
            VMResponse<VMSoItem> response = new VMResponse<VMSoItem>();

            using IDbContextTransaction dbTran = db.Database.BeginTransaction();
            {
                try
                {
                    VMSoOrder? order = (
                        from ord in db.SoOrders
                        where ord.SoOrderId == data.SoOrderId
                        select new VMSoOrder
                        {
                            SoOrderId = ord.SoOrderId,
                            OrderNo = ord.OrderNo,
                            OrderDate = ord.OrderDate,
                            ComCustomerId = ord.ComCustomerId,
                            Address = ord.Address
                        }
                        ).FirstOrDefault();

                    if (order == null)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.Message = $"{HttpStatusCode.BadRequest} - No order data was Found!";
                    }

                    SoItem newData = new SoItem
                    {
                        SoOrderId = data.SoOrderId * -1,
                        ItemName = data.ItemName,
                        Quantity = data.Quantity,
                        Price = data.Price
                    };

                    db.Add(newData);
                    db.SaveChanges();

                    dbTran.Commit();

                    response.StatusCode = HttpStatusCode.Created;
                    response.Message = $"{HttpStatusCode.Created} - Temporary Item for Order Id {order!.SoOrderId} successfully created!";
                }
                catch (Exception ex)
                {
                    dbTran.Rollback();
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Message = $"{HttpStatusCode.InternalServerError} - From DAItem.AddTemporaryItems: {ex.Message}";
                }
            }

            return response;
        }

        public VMResponse<VMSoItem> UpdateItem(VMSoItem data)
        {
            VMResponse<VMSoItem> response = new VMResponse<VMSoItem>();

            using IDbContextTransaction dbTran = db.Database.BeginTransaction();
            {
                try
                {

                }
                catch (Exception ex)
                {
                    dbTran.Rollback();
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Message = $"{HttpStatusCode.InternalServerError} - From DAItem.UpdateItem: {ex.Message}";
                }
            }

            return response;
        }
    }
}
