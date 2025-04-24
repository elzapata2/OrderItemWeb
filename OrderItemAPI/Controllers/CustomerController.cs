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
    public class CustomerController : ControllerBase
    {
        private DACustomer cust;

        public CustomerController(OlDevContext _db)
        {
            cust = new DACustomer(_db);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetAllCustomers()
        {
            VMResponse<List<VMComCustomer>?> response = new VMResponse<List<VMComCustomer>?>();

            try
            {
                response = await Task.Run(() =>  cust.GetAllCustomers());

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
                response.Message = $"{HttpStatusCode.InternalServerError} - From CustomerController.GetAllCustomers: {ex.Message}";
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}
