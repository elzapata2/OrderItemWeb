using System.Drawing.Printing;
using System.Net;
using System.Text;
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
                    apiResponse.Message = $"{apiResponse.StatusCode} - From OrderModel.GetOrders: Can't reach Order API";
                }
            }
            catch (Exception ex)
            {
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiResponse.Message = $"{HttpStatusCode.InternalServerError} - From OrderModel.GetOrders: Error when trying to reach Order API, {ex.Message}";
            }

            return apiResponse;
        }

        public async Task<VMResponse<VMSoOrder>> GetOrderWithItems(long orderId, int page, int pageSize, List<long>? exceptionIds)
        {
            VMResponse<VMSoOrder> apiResponse = new VMResponse<VMSoOrder>();

            try
            {
                string addedUrl = "";
                if (exceptionIds != null && exceptionIds.Count > 0)
                {
                    for (int i = 0; i < exceptionIds.Count; i++)
                    {
                        addedUrl += $"&exceptionIds={exceptionIds[i]}";
                    }
                }

                HttpResponseMessage apiResponseMsg =
                    await httpClient.GetAsync($"{apiUrl}Order/GetOrderWithItems?orderId={orderId}&page={page}&itemsperpage={pageSize}{addedUrl}");
                string apiMsgContent = apiResponseMsg.Content.ReadAsStringAsync().Result;

                if (apiMsgContent != string.Empty)
                {
                    apiResponse = JsonConvert.DeserializeObject<VMResponse<VMSoOrder>>(apiMsgContent)!;
                }
                else
                {
                    apiResponse.StatusCode = apiResponseMsg.StatusCode;
                    apiResponse.Message = $"{apiResponse.StatusCode} - From OrderModel.GetOrderWithItems: Can't reach Order API";
                }
            }
            catch(Exception ex)
            {
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiResponse.Message = $"{HttpStatusCode.InternalServerError} - From OrderModel.GetOrderWithItems: Error when trying to reach Order API, {ex.Message}";
            }

            return apiResponse;
        }

        public async Task<VMResponse<VMSoOrder>> AddOrder(VMSoOrder data)
        {
            VMResponse<VMSoOrder> apiResponse = new VMResponse<VMSoOrder>();

            try
            {
                jsonData = JsonConvert.SerializeObject(data);
                content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage apiResponseMsg = await httpClient.PostAsync($"{apiUrl}Order/AddOrder", content);
                string apiMsgContent = apiResponseMsg.Content.ReadAsStringAsync().Result;

                if (apiMsgContent != string.Empty)
                {
                    apiResponse = JsonConvert.DeserializeObject<VMResponse<VMSoOrder>>(apiMsgContent)!;
                }
                else
                {
                    apiResponse.StatusCode = apiResponseMsg.StatusCode;
                    apiResponse.Message = $"{apiResponse.StatusCode} - From OrderModel.AddOrder: Can't reach Order API";
                }
            }
            catch (Exception ex)
            {
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiResponse.Message = $"{HttpStatusCode.InternalServerError} - From OrderModel.AddOrder: Error when trying to reach Order API, {ex.Message}";
            }

            return apiResponse;
        }

        public async Task<VMResponse<VMSoOrder>> DeleteOrder(long orderId)
        {
            VMResponse<VMSoOrder> apiResponse = new VMResponse<VMSoOrder>();

            try
            {
                HttpResponseMessage apiResponseMsg =
                    await httpClient.DeleteAsync($"{apiUrl}Order/DeleteOrder?orderId={orderId}");
                string apiMsgContent = apiResponseMsg.Content.ReadAsStringAsync().Result;

                if (apiMsgContent != string.Empty)
                {
                    apiResponse = JsonConvert.DeserializeObject<VMResponse<VMSoOrder>>(apiMsgContent)!;
                }
                else
                {
                    apiResponse.StatusCode = apiResponseMsg.StatusCode;
                    apiResponse.Message = $"{apiResponse.StatusCode} - From OrderModel.DeleteOrder: Can't reach Order API";
                }
            }
            catch (Exception ex)
            {
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiResponse.Message = $"{HttpStatusCode.InternalServerError} - From OrderModel.DeleteOrder: Error when trying to reach Order API, {ex.Message}";
            }

            return apiResponse;
        }

        public async Task<VMResponse<VMSoOrder>> UpdateOrder(VMSoOrder data)
        {
            VMResponse<VMSoOrder> apiResponse = new VMResponse<VMSoOrder>();

            try
            {
                jsonData = JsonConvert.SerializeObject(data);
                content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage apiResponseMsg = await httpClient.PutAsync($"{apiUrl}Order/UpdateOrder", content);
                string apiMsgContent = apiResponseMsg.Content.ReadAsStringAsync().Result;

                if (apiMsgContent != string.Empty)
                {
                    apiResponse = JsonConvert.DeserializeObject<VMResponse<VMSoOrder>>(apiMsgContent)!;
                }
                else
                {
                    apiResponse.StatusCode = apiResponseMsg.StatusCode;
                    apiResponse.Message = $"{apiResponse.StatusCode} - From OrderModel.UpdateOrder: Can't reach Order API";
                }
            }
            catch (Exception ex)
            {
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiResponse.Message = $"{HttpStatusCode.InternalServerError} - From OrderModel.UpdateOrder: Error when trying to reach Order API, {ex.Message}";
            }

            return apiResponse;
        }
    }
}
