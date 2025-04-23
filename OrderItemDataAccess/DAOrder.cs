using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OrderItemViewModel;
using Microsoft.EntityFrameworkCore.Storage;
using OrderItemModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OrderItemDataAccess
{
    public class DAOrder
    {
        private OlDevContext db;

        public DAOrder (OlDevContext _db)
        {
            db = _db;
        }

        public VMResponse<List<VMSoOrder>?> GetOrders(string? keyword, DateTime? searchDate, int page, int itemsPerPage)
        {
            VMResponse<List<VMSoOrder>?> response = new VMResponse<List<VMSoOrder>?>();

            try
            {
                List<VMSoOrder>? data = (
                    from ord in db.SoOrders
                    join cust in db.ComCustomers on ord.ComCustomerId equals cust.ComCustomerId
                    where
                    (ord.OrderNo.Contains((string.IsNullOrEmpty(keyword) ? "" : keyword)) || cust.CustomerName!.Contains((string.IsNullOrEmpty(keyword) ? "" : keyword))) && 
                    (searchDate == null || ord.OrderDate == searchDate)
                    select new VMSoOrder
                    {
                        SoOrderId = ord.SoOrderId,
                        OrderNo = ord.OrderNo,
                        OrderDate = ord.OrderDate,
                        ComCustomerId = ord.ComCustomerId,
                        ComCustomerName = cust.CustomerName,
                        Address = ord.Address,
                        //Items = (
                        //    from item in db.SoItems
                        //    where item.SoOrderId == ord.SoOrderId
                        //    select new VMSoItem
                        //    {
                        //        SoItemId = item.SoItemId,
                        //        SoOrderId = item.SoOrderId,
                        //        ItemName = item.ItemName,
                        //        Quantity = item.Quantity,
                        //        Price = item.Price
                        //    }
                        //    ).ToList()
                    }
                    ).Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();

                if (data != null && data.Count > 0)
                {
                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = $"{HttpStatusCode.OK} - Successfully fetched {data.Count} Order Data!";
                    response.Data = data;
                }
                else
                {
                    response.StatusCode = HttpStatusCode.NoContent;
                    response.Message = $"{HttpStatusCode.NoContent} - No order data was Found!";
                }

            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From DAOrder.GetOrders: {ex.Message}";
            }

            return response;
        }

        public VMResponse<VMSoOrder> AddOrder(VMSoOrder data)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            using IDbContextTransaction dbTran = db.Database.BeginTransaction();
            {
                try
                {                  
                    SoOrder newData = new SoOrder
                    {
                        OrderNo = data.OrderNo,
                        OrderDate = data.OrderDate,
                        ComCustomerId = data.ComCustomerId,
                        Address = data.Address
                    };

                    db.Add(newData);
                    db.SaveChanges();

                    if (data.Items != null && data.Items.Count > 0)
                    {
                        foreach (VMSoItem itemData in data.Items)
                        {
                            SoItem newItemData = new SoItem
                            {
                                SoOrderId = newData.SoOrderId,
                                ItemName = itemData.ItemName,
                                Quantity = itemData.Quantity,
                                Price = itemData.Price
                            };

                            db.Add(newItemData);
                            db.SaveChanges();
                        }
                    }
                    
                    dbTran.Commit();

                    response.StatusCode = HttpStatusCode.Created;
                    response.Message = $"{HttpStatusCode.Created} - Order successfully created!";
                }
                catch (Exception ex)
                {
                    dbTran.Rollback();
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Message = $"{HttpStatusCode.InternalServerError} - From DAOrder.AddOrder: {ex.Message}";
                }

                return response;
            }
        }

        public VMResponse<VMSoOrder> UpdateOrder(VMSoOrder data)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            using IDbContextTransaction dbTran = db.Database.BeginTransaction();
            {
                try
                {
                    VMSoOrder? existingData = (
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

                    if (existingData == null)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.Message = $"{HttpStatusCode.BadRequest} - Order data not found!";
                        return response;
                    }

                    if (data.Items != null && data.Items.Count > 0)
                    {
                        foreach (VMSoItem itemData in data.Items)
                        {
                            VMSoItem? existingItemData = (
                                from item in db.SoItems
                                where item.SoItemId == itemData.SoItemId
                                select new VMSoItem
                                {
                                    SoItemId = item.SoItemId,
                                    SoOrderId = item.SoOrderId,
                                    ItemName = item.ItemName,
                                    Quantity = item.Quantity,
                                    Price = item.Price
                                }
                                ).FirstOrDefault();

                            if (existingItemData == null)
                            {
                                dbTran.Rollback();
                                response.StatusCode = HttpStatusCode.BadRequest;
                                response.Message = $"{HttpStatusCode.BadRequest} - Item data not found!";
                                return response;
                            }

                            SoItem updatedItemData = new SoItem
                            {
                                SoItemId = existingItemData.SoItemId,
                                SoOrderId = existingItemData.SoOrderId,
                                ItemName = itemData.ItemName,
                                Quantity = itemData.Quantity,
                                Price = itemData.Price
                            };

                            db.Update(updatedItemData);
                            db.SaveChanges();
                        }
                    }

                    SoOrder newData = new SoOrder
                    {
                        SoOrderId = existingData.SoOrderId,
                        OrderNo = data.OrderNo,
                        OrderDate = data.OrderDate,
                        ComCustomerId = data.ComCustomerId,
                        Address = data.Address
                    };

                    db.Update(newData);
                    db.SaveChanges();

                    dbTran.Commit();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = $"{HttpStatusCode.OK} - Order successfully updated!";
                }
                catch (Exception ex)
                {
                    dbTran.Rollback();
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Message = $"{HttpStatusCode.InternalServerError} - From DAOrder.UpdateOrder: {ex.Message}";
                }

                return response;
            }
        }

        public VMResponse<VMSoOrder> DeleteOrder(long orderId)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            using IDbContextTransaction dbTran = db.Database.BeginTransaction();
            {
                try
                {
                    VMSoOrder? existingData = (
                        from ord in db.SoOrders
                        where ord.SoOrderId == orderId
                        select new VMSoOrder
                        {
                            SoOrderId = ord.SoOrderId,
                            OrderNo = ord.OrderNo,
                            OrderDate = ord.OrderDate,
                            ComCustomerId = ord.ComCustomerId,
                            Address = ord.Address
                        }
                        ).FirstOrDefault();

                    if (existingData == null)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.Message = $"{HttpStatusCode.BadRequest} - Order data not found!";
                        return response;
                    }

                    if (existingData.Items != null && existingData.Items.Count > 0)
                    {
                        foreach (VMSoItem itemData in existingData.Items)
                        {
                            VMSoItem? existingItemData = (
                                from item in db.SoItems
                                where item.SoItemId == itemData.SoItemId
                                select new VMSoItem
                                {
                                    SoItemId = item.SoItemId,
                                    SoOrderId = item.SoOrderId,
                                    ItemName = item.ItemName,
                                    Quantity = item.Quantity,
                                    Price = item.Price
                                }
                                ).FirstOrDefault();

                            if (existingItemData == null)
                            {
                                dbTran.Rollback();
                                response.StatusCode = HttpStatusCode.BadRequest;
                                response.Message = $"{HttpStatusCode.BadRequest} - Item data not found!";
                                return response;
                            }

                            SoItem removedItemData = new SoItem
                            {
                                SoItemId = existingItemData.SoItemId,
                                SoOrderId = existingItemData.SoOrderId,
                                ItemName = existingItemData.ItemName,
                                Quantity = existingItemData.Quantity,
                                Price = existingItemData.Price
                            };

                            db.Update(removedItemData);
                            db.SaveChanges();
                        }
                    }

                    SoOrder removedData = new SoOrder
                    {
                        SoOrderId = existingData.SoOrderId,
                        OrderNo = existingData.OrderNo,
                        OrderDate = existingData.OrderDate,
                        ComCustomerId = existingData.ComCustomerId,
                        Address = existingData.Address
                    };

                    db.Remove(removedData);
                    db.SaveChanges();

                    dbTran.Commit();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = $"{HttpStatusCode.OK} - Order successfully deleted!";
                }
                catch (Exception ex)
                {
                    dbTran.Rollback();
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Message = $"{HttpStatusCode.InternalServerError} - From DAOrder.UpdateOrder: {ex.Message}";
                }

                return response;
            }
        }
    }
}
