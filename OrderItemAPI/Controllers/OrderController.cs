using System.Net;
using OrderItemViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderItemDataAccess;
using OrderItemModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OrderItemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private DAOrder order;

        public OrderController(OlDevContext _db)
        {
            order = new DAOrder(_db);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetOrders(string? keyword, string? searchDate, int page, int itemsperpage)
        {
            VMResponse<List<VMSoOrder>?> response = new VMResponse<List<VMSoOrder>?>();

            try
            {
                if (string.IsNullOrEmpty(searchDate))
                {
                    response = await Task.Run(() => order.GetOrders(keyword, null, page, itemsperpage));
                }
                else
                {
                    DateTime parsedDate;

                    if (DateTime.TryParse(searchDate, out parsedDate))
                        response = await Task.Run(() => order.GetOrders(keyword, parsedDate, page, itemsperpage));
                    else
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.Message = $"{HttpStatusCode.BadRequest} - Search Date is invalid!";
                        return BadRequest(response);
                    }
                        
                }

                //response = await Task.Run(() => order.GetOrders(keyword, DateTime.Parse(searchDate), page, itemsperpage));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.GetOrders: {ex.Message}";
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetOrderWithItems(long orderId, int page, int itemsPerPage)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            try
            {
                response = await Task.Run(() => order.GetOrderWithItems(orderId, page, itemsPerPage));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response);
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(response);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.GetOrderWithItems: {ex.Message}";
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> AddOrder(VMSoOrder data)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            try
            {
                response = await Task.Run(() => order.AddOrder(data));

                if (response.StatusCode != HttpStatusCode.Created)
                {
                    return StatusCode((int)response.StatusCode, response);
                }

                return Created("api/Order", response);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.AddOrder: {ex.Message}";
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpPut("[action]")]
        public async Task<ActionResult> UpdateOrder(VMSoOrder data)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            try
            {
                response = await Task.Run(() => order.UpdateOrder(data));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response);
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(response);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response);
                }                   
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.AddOrder: {ex.Message}";
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpDelete("[action]")]
        public async Task<ActionResult> DeleteOrder(long orderId)
        {
            VMResponse<VMSoOrder> response = new VMResponse<VMSoOrder>();

            try
            {
                response = await Task.Run(() => order.DeleteOrder(orderId));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response);
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return BadRequest(response);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From OrderController.AddOrder: {ex.Message}";
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}
