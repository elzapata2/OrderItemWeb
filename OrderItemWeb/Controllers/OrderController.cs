using System.Net;
using Microsoft.AspNetCore.Mvc;
using OrderItemViewModel;
using OrderItemWeb.Models;

namespace OrderItemWeb.Controllers
{
    public class OrderController : Controller
    {
        private OrderModel order;

        private int defaultPage = 1;
        private int defaultPageSize = 4;

        public OrderController(IConfiguration _config)
        {
            order = new OrderModel(_config);
        }

        public async Task<IActionResult> Index(string? keyword, string? searchDate, int? page, int? pageSize)
        {
            VMResponse<List<VMSoOrder>?> response = new VMResponse<List<VMSoOrder>?>();

            try
            {
                response = await order.GetOrders(keyword, searchDate, (page == null) ? defaultPage : (int)page, (pageSize == null) ? defaultPageSize : (int)pageSize);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.Index: {ex.Message}";
                response.Data = new List<VMSoOrder>();
            }

            return View(response.Data);
        }

        public async Task<IActionResult> CreateEdit(long? orderId, int? page, int? pageSize)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            page = (page == null) ? defaultPage : page;
            pageSize = (pageSize == null) ? defaultPageSize : pageSize;

            if (orderId == null)
            {
                response.Data = new VMSoOrder();
                return View(response.Data);
            }

            try
            {
                response = await order.GetOrderWithItems((long)orderId, (int)page, (int)pageSize);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.CreateEdit: {ex.Message}";
                response.Data = new VMSoOrder();
            }

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
    }
}
