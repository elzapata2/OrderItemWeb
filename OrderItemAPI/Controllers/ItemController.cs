using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderItemDataAccess;
using OrderItemModel;
using OrderItemViewModel;

namespace OrderItemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private DAItem item;

        public ItemController(OlDevContext _db)
        {
            item = new DAItem(_db);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetTemporaryItemsByOrderId(long orderId)
        {
            VMResponse<List<VMSoItem>?> response = new VMResponse<List<VMSoItem>?>();

            try
            {
                response = await Task.Run(() => item.GetTemporaryItemsByOrderId(orderId));

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
            catch(Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From ItemController.GetTemporaryItemsByOrderId: {ex.Message}";
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> AddTemporaryItem(VMSoItem data)
        {
            VMResponse<VMSoItem> response = new VMResponse<VMSoItem>();

            try
            {
                response = await Task.Run(() => item.AddTemporaryItems(data));

                if (response.StatusCode != HttpStatusCode.Created)
                {
                    return StatusCode((int)response.StatusCode, response);
                }

                return Created("api/Item", response);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = $"{HttpStatusCode.InternalServerError} - From ItemController.AddTemporaryItem: {ex.Message}";
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }
    }
}
