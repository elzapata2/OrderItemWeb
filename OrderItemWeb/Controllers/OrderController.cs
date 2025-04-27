using System.Net;
using Azure;
using Microsoft.AspNetCore.Mvc;
using OrderItemViewModel;
using OrderItemWeb.Models;

namespace OrderItemWeb.Controllers
{
    public class OrderController : Controller
    {
        private OrderModel order;
        private CustomerModel cust;

        private int defaultPage = 1;
        private int defaultPageSize = 4;

        public OrderController(IConfiguration _config)
        {
            order = new OrderModel(_config);
            cust = new CustomerModel(_config);
        }

        public async Task<VMResponse<VMSoOrder>> GetOrderItemsAsync (long orderId, int page, int pageSize, List<long>? exceptionIds)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            try
            {
                response = await order.GetOrderWithItems(orderId, page, pageSize, exceptionIds);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.GetOrderItems: {ex.Message}";
                response.Data = new VMSoOrder();
            }

            return response;
        }

        public async Task<IActionResult> Index(string? keyword, string? searchDate, int? page, int? pageSize)
        {
            VMResponse<List<VMSoOrder>?> response = new VMResponse<List<VMSoOrder>?>();

            page = (page == null) ? defaultPage : page;
            pageSize = (pageSize == null) ? defaultPageSize : pageSize;

            try
            {
                response = await order.GetOrders(keyword, searchDate, (int)page, (int)pageSize);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.Index: {ex.Message}";
                response.Data = new List<VMSoOrder>();
            }

            ViewBag.Keyword = keyword; 
            ViewBag.SearchDate = searchDate;
            ViewBag.Page = page; 
            ViewBag.PageSize = pageSize;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                ViewBag.TotalOrders = response.TotalData;
                ViewBag.TotalPages = (ViewBag.TotalOrders % ViewBag.PageSize != 0) ? ViewBag.TotalOrders / ViewBag.PageSize + 1 : ViewBag.TotalOrders / ViewBag.PageSize;
            }
            else
            {
                ViewBag.TotalOrders = 0;
                ViewBag.TotalPages = 1;
            }

            ViewBag.Title = "Sale Orders";

            return View(response.Data);
        }

        public async Task<IActionResult> Create(int? page, int? pageSize)
        {
            page = (page == null) ? defaultPage : page;
            pageSize = (pageSize == null) ? defaultPageSize : pageSize;

            VMResponse<List<VMComCustomer>?> customers = new VMResponse<List<VMComCustomer>?>();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            try
            {
                customers = await cust.GetAllCustomers();

                if (customers.StatusCode == HttpStatusCode.OK)
                {
                    ViewBag.Customers = customers.Data;
                }
                else if (customers.StatusCode == HttpStatusCode.NoContent)
                {
                    ViewBag.Customers = null;
                }
                else
                {
                    throw new Exception(customers.Message);
                }
            }
            catch (Exception ex)
            {
                customers.StatusCode = HttpStatusCode.InternalServerError;
                customers.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.CreateEdit: {ex.Message}";
                customers.Data = new List<VMComCustomer>();
            }

            ViewBag.Title = "Add Sale Orders";

            return View();
        }

        public async Task<IActionResult> CreateEdit(long orderId, int? page, int? pageSize)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            page = (page == null) ? defaultPage : page;
            pageSize = (pageSize == null) ? defaultPageSize : pageSize;

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = null;
            ViewBag.OrderDate = null;
            ViewBag.CustomerId = 1;
            ViewBag.Address = null;
            ViewBag.TotalItem = 0;
            ViewBag.TotalAmount = 0;

            try
            {
                VMResponse<List<VMComCustomer>?> customers = await cust.GetAllCustomers();

                if (customers.StatusCode == HttpStatusCode.OK)
                {
                    ViewBag.Customers = customers.Data;
                }
                else if (customers.StatusCode == HttpStatusCode.NoContent)
                {
                    ViewBag.Customers = null;
                }
                else
                {
                    throw new Exception(customers.Message);
                }

                //if (orderId == null)
                //{
                //    response.Data = new VMSoOrder();
                //    return View(response.Data);
                //}

           
                response = await order.GetOrderWithItems(orderId, (int)page, (int)pageSize, null);

                ViewBag.OrderNo = response.Data!.OrderNo;
                ViewBag.OrderDate = response.Data.OrderDate.Date.ToString("yyyy-MM-dd");
                ViewBag.CustomerId = response.Data.ComCustomerId;
                ViewBag.Address = response.Data.Address;
                ViewBag.TotalItem = response.TotalItem;
                ViewBag.TotalAmount = response.TotalAmount;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.CreateEdit: {ex.Message}";
                response.Data = new VMSoOrder();
            }

            ViewBag.OrderId = orderId;

            ViewBag.TotalPages = (response.TotalData % ViewBag.PageSize != 0) ? response.TotalData / ViewBag.PageSize + 1 : response.TotalData / ViewBag.PageSize;

            ViewBag.Title = "Edit Sale Orders";

            return View(response.Data);
        }

        public IActionResult DeleteOrder(string orderNo, long orderId)
        {
            ViewBag.OrderNo = orderNo;
            ViewBag.OrderId = orderId;
            ViewBag.Title = "Delete Order";
            return View();
        }

        [HttpPost]
        public async Task<VMResponse<VMSoOrder>> AddOrderAsync([FromBody] VMSoOrder data)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            try
            {
                response = await order.AddOrder(data);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.AddOrder: {ex.Message}";
            }

            return response;
        }

        [HttpPost]
        public async Task<VMResponse<VMSoOrder>> DeleteOrderAsync(VMSoOrder data)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            try
            {
                response = await order.DeleteOrder(data.SoOrderId);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.DeleteOrderAsync: {ex.Message}";
            }

            return response;
        }

        [HttpPost]
        public async Task<VMResponse<VMSoOrder>> UpdateOrderAsync([FromBody] VMSoOrder data)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            try
            {
                response = await order.UpdateOrder(data);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.UpdateOrderAsync: {ex.Message}";
            }

            return response;
        }
    }
}
