using System.Net;
using Azure;
using Newtonsoft.Json;
using OrderItemViewModel;

namespace OrderItemWeb.Models
{
    public class CustomerModel
    {
        private readonly HttpClient httpClient;
        private readonly string apiUrl;

        public CustomerModel(IConfiguration _config)
        {
            httpClient = new HttpClient();
            apiUrl = _config["ApiUrl"]!;
        }
        public async Task<VMResponse<List<VMComCustomer>?>> GetAllCustomers()
        {
            VMResponse<List<VMComCustomer>?> apiResponse = new VMResponse<List<VMComCustomer>?>();

            try
            {
                HttpResponseMessage apiResponseMsg = await httpClient.GetAsync($"{apiUrl}Customer/GetAllCustomers");
                string apiMsgContent = apiResponseMsg.Content.ReadAsStringAsync().Result;

                if (apiMsgContent != string.Empty)
                {
                    apiResponse = JsonConvert.DeserializeObject<VMResponse<List<VMComCustomer>?>>(apiMsgContent)!;
                }
                else
                {
                    apiResponse.StatusCode = apiResponseMsg.StatusCode;
                    apiResponse.Message = $"{apiResponse.StatusCode} - From CustomerModel.GetAllCustomers: Can't reach Customer API";
                }
            }
            catch (Exception ex)
            {
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiResponse.Message = $"{HttpStatusCode.InternalServerError} - From CustomerModel.GetAllCustomers: Error when trying to reach Customer API, {ex.Message}";
            }

            return apiResponse;
        }
    }
}
