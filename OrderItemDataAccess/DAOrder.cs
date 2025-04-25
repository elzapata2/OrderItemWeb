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
using System.Security.Cryptography;

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
                var query = (
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
                    });

                long totalCount = query.Count();
                List<VMSoOrder>? data = query.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).OrderByDescending(d => d.OrderDate).ToList();

                //List<VMSoOrder>? data = (
                //    from ord in db.SoOrders
                //    join cust in db.ComCustomers on ord.ComCustomerId equals cust.ComCustomerId
                //    where
                //    (ord.OrderNo.Contains((string.IsNullOrEmpty(keyword) ? "" : keyword)) || cust.CustomerName!.Contains((string.IsNullOrEmpty(keyword) ? "" : keyword))) && 
                //    (searchDate == null || ord.OrderDate == searchDate)
                //    select new VMSoOrder
                //    {
                //        SoOrderId = ord.SoOrderId,
                //        OrderNo = ord.OrderNo,
                //        OrderDate = ord.OrderDate,
                //        ComCustomerId = ord.ComCustomerId,
                //        ComCustomerName = cust.CustomerName,
                //        Address = ord.Address,
                //        //Items = (
                //        //    from item in db.SoItems
                //        //    where item.SoOrderId == ord.SoOrderId
                //        //    select new VMSoItem
                //        //    {
                //        //        SoItemId = item.SoItemId,
                //        //        SoOrderId = item.SoOrderId,
                //        //        ItemName = item.ItemName,
                //        //        Quantity = item.Quantity,
                //        //        Price = item.Price
                //        //    }
                //        //    ).ToList()
                //    }
                //    ).Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();

                if (data != null && data.Count > 0)
                {
                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = $"{HttpStatusCode.OK} - Successfully fetched {data.Count} of {totalCount} Order Data!";
                    response.Data = data;
                    response.TotalData = totalCount;
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

        //public List<VMSoItem>? GetTempDeletedItems(long itemId)
        //{
        //    return (
        //        from vw in db.ViewSoItems
        //        where vw.SoItemId == itemId
        //        select new VMSoItem
        //        {
        //            SoItemId = vw.SoItemId,
        //            SoOrderId = vw.SoOrderId,
        //            ItemName = vw.ItemName,
        //            Quantity = vw.Quantity,
        //            Price = vw.Price
        //        }
        //        ).ToList();
        //}

        public VMResponse<VMSoOrder> GetOrderWithItems(long orderId, int page, int itemsPerPage, List<long>? exceptionIds)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            try
            {
                var queryItems = (
                    from item in db.SoItems
                    where item.SoOrderId == orderId
                    select new VMSoItem
                    {
                        SoItemId = item.SoItemId,
                        SoOrderId = item.SoOrderId,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }
                    );

                VMSoOrder? data = new VMSoOrder();

                if (exceptionIds != null)
                {
                    data = (
                        from ord in db.SoOrders
                        join cust in db.ComCustomers on ord.ComCustomerId equals cust.ComCustomerId
                        where ord.SoOrderId == orderId
                        select new VMSoOrder
                        {
                            SoOrderId = ord.SoOrderId,
                            OrderNo = ord.OrderNo,
                            OrderDate = ord.OrderDate,
                            ComCustomerId = ord.ComCustomerId,
                            ComCustomerName = cust.CustomerName,
                            Address = ord.Address,
                            Items = queryItems.Where(x => !exceptionIds.Contains(x.SoItemId)).Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList()
                            //queryItems.ToList()
                            //).ToList()
                        }
                        ).FirstOrDefault();
                }
                else
                {
                    data = (
                        from ord in db.SoOrders
                        join cust in db.ComCustomers on ord.ComCustomerId equals cust.ComCustomerId
                        where ord.SoOrderId == orderId
                        select new VMSoOrder
                        {
                            SoOrderId = ord.SoOrderId,
                            OrderNo = ord.OrderNo,
                            OrderDate = ord.OrderDate,
                            ComCustomerId = ord.ComCustomerId,
                            ComCustomerName = cust.CustomerName,
                            Address = ord.Address,
                            Items = queryItems.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList()
                            //queryItems.ToList()
                            //).ToList()
                        }
                        ).FirstOrDefault();
                }
                    

                if (data != null)
                {
                    //var queryTempItems = (
                    //    from vw in db.ViewSoItems
                    //    select new VMSoItem
                    //    {
                    //        SoItemId = vw.SoItemId,
                    //        SoOrderId = vw.SoOrderId,
                    //        ItemName = vw.ItemName,
                    //        Quantity = vw.Quantity,
                    //        Price = vw.Price
                    //    }
                    //    );

                    //List<VMSoItem>?  tempItems = queryTempItems.ToList();

                    //List<VMSoItem>? tempAddedItems = tempItems.Where(x => x.SoItemId == 0 && x.SoOrderId == orderId).ToList();

                    //List<VMSoItem>? tempEditedItems = tempItems.Where(x => x.SoItemId != 0 && x.SoOrderId == orderId).ToList();

                    //List<VMSoItem>? tempDeletedItems = tempItems.Where(x => x.SoItemId != 0 && x.SoOrderId == 0).ToList();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = $"{HttpStatusCode.OK} - Successfully fetched the data";
                    response.Data = data;
                    
                    //if (tempDeletedItems != null )
                    //{
                    //    foreach (VMSoItem item in tempDeletedItems)
                    //    {
                    //        response.Data.Items = response.Data.Items.Where(x => x.SoItemId != item.SoItemId).ToList();
                    //    }
                    //}

                    
                    response.TotalData = queryItems.Count();
                    foreach (VMSoItem dataItem in queryItems.ToList())
                    {
                        response.TotalItem += dataItem.Quantity;
                        response.TotalAmount += dataItem.Quantity * dataItem.Price;
                    }
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
                response.Message = $"{HttpStatusCode.InternalServerError} - From DAOrder.GetOrderWithItems: {ex.Message}";
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
                            if (itemData.SoItemId < 0)
                            {
                                SoItem newItem = new SoItem
                                {
                                    SoOrderId = data.SoOrderId,
                                    ItemName = itemData.ItemName,
                                    Quantity = itemData.Quantity,
                                    Price = itemData.Price
                                };

                                db.Add(newItem);
                                db.SaveChanges();
                            }
                            else if (itemData.SoOrderId > 0)
                            {
                                VMSoItem? existingItemData = (
                                    from item in db.SoItems
                                    where item.SoItemId == itemData.SoItemId
                                    select new VMSoItem
                                    {
                                        SoItemId = item.SoOrderId,
                                        SoOrderId = item.SoOrderId,
                                        ItemName = item.ItemName,
                                        Quantity = itemData.Quantity,
                                        Price = itemData.Price
                                    }
                                    ).FirstOrDefault();

                                if (existingItemData == null)
                                {
                                    dbTran.Rollback();
                                    response.StatusCode = HttpStatusCode.BadRequest;
                                    response.Message = $"{HttpStatusCode.BadRequest} - No Item data found!";
                                    return response;
                                }

                                SoItem updatedItemData = new SoItem
                                {
                                    SoItemId = itemData.SoItemId,
                                    SoOrderId = data.SoOrderId,
                                    ItemName = itemData.ItemName,
                                    Quantity = itemData.Quantity,
                                    Price = itemData.Price
                                };

                                db.Update(updatedItemData);
                                db.SaveChanges();
                            }
                            else
                            {
                                VMSoItem? existingItemData = (
                                    from item in db.SoItems
                                    where item.SoItemId == itemData.SoItemId
                                    select new VMSoItem
                                    {
                                        SoItemId = item.SoOrderId,
                                        SoOrderId = item.SoOrderId,
                                        ItemName = item.ItemName,
                                        Quantity = itemData.Quantity,
                                        Price = itemData.Price
                                    }
                                    ).FirstOrDefault();

                                if (existingItemData == null)
                                {
                                    dbTran.Rollback();
                                    response.StatusCode = HttpStatusCode.BadRequest;
                                    response.Message = $"{HttpStatusCode.BadRequest} - No Item data found!";
                                    return response;
                                }

                                SoItem deletedItemData = new SoItem
                                {
                                    SoItemId = itemData.SoItemId,
                                    SoOrderId = data.SoOrderId,
                                    ItemName = itemData.ItemName,
                                    Quantity = itemData.Quantity,
                                    Price = itemData.Price
                                };

                                db.Remove(deletedItemData);
                                db.SaveChanges();
                            }                                                         
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
                            Address = ord.Address,
                            Items = (
                                from item in db.SoItems
                                where item.SoOrderId == ord.SoOrderId
                                select new VMSoItem
                                {
                                    SoItemId = item.SoItemId,
                                    SoOrderId = item.SoOrderId,
                                    ItemName = item.ItemName,
                                    Quantity = item.Quantity,
                                    Price = item.Price
                                }
                                ).ToList()
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

                            db.Remove(removedItemData);
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
