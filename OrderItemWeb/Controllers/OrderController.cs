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
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController: {ex.Message}";
                response.Data = new List<VMSoOrder>();
            }

            return View(response.Data);
        }

        public IActionResult CreateEdit(long? orderId)
        {
            return View();
        }

        public IActionResult DeleteOrder(string orderNo)
        {
            ViewBag.OrderNo = orderNo;
            ViewBag.Title = "Delete Order";
            return View();
        }
    }
}
