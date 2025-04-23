using System.Net;
using Newtonsoft.Json;
using OrderItemViewModel;

namespace OrderItemWeb.Models
{
    public class OrderModel
    {
        private readonly HttpClient httpClient;
        private readonly string apiUrl;

        private HttpContent content;
        private string jsonData;

        public OrderModel(IConfiguration _config)
        {
            httpClient = new HttpClient();
            apiUrl = _config["ApiUrl"]!;
        }

        public async Task<VMResponse<List<VMSoOrder>?>> GetOrders(string? keyword, string? searchDate, int page, int pageSize)
        {
            VMResponse<List<VMSoOrder>?> apiResponse = new VMResponse<List<VMSoOrder>?>();

            if (keyword == null)
                keyword = "";
            if (searchDate == null)
                searchDate = "";

            try
            {
                HttpResponseMessage apiResponseMsg = 
                    await httpClient.GetAsync($"{apiUrl}Order/GetOrders?keyword={keyword}&searchDate={searchDate}&page={page}&itemsperpage={pageSize}");
                string apiMsgContent = apiResponseMsg.Content.ReadAsStringAsync().Result;

                if (apiMsgContent != string.Empty)
                {
                    apiResponse = JsonConvert.DeserializeObject<VMResponse<List<VMSoOrder>?>>(apiMsgContent)!;
                }
                else
                {
                    apiResponse.StatusCode = apiResponseMsg.StatusCode;
                    apiResponse.Message = $"{apiResponse.StatusCode} - From OrderModel: Can't reach Order API";
                }
            }
            catch (Exception ex)
            {
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiResponse.Message = $"{HttpStatusCode.InternalServerError} - From OrderModel: Error when trying to reach Order API, {ex.Message}";
            }

            return apiResponse;
        }
    }
}
