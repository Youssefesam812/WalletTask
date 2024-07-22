using System.Text.Json;
using System.Text;
using TrialWork.Models;
using static TrialWork.Models.GetMerchantTokenRequest;
using static TrialWork.Models.StartTransactionRequest;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Logging;

namespace TrialWork.Controllers
{
    public class WalletController
    {
        private readonly ILogger<WalletController> _logger;
        private readonly HttpClient _httpClient;

       public enum methodAPI
        {
            Refund,
            GetToken,
            Create
        }

        public WalletController(ILogger<WalletController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }
        public async Task<WalletResponse> StartTransaction(WalletRequest walletRequest)
        {


            var ReturnSuccess = "Success";
            var ReturnError = "Failure";
            var Operator = "NAS Wallet";
            DateTime date = DateTime.Now;
            string formattedDate = date.ToString("yyyy-MM-dd");

            var url = "https://uatgw1.nasswallet.com";

            try
            {
               
                    
                     var merchantTokenResponse = await CallGetMerchantToken(_httpClient, url, walletRequest.Credentials);

                  if (merchantTokenResponse != null && merchantTokenResponse.Message == $"{ReturnSuccess}")
                        {
                        _logger.LogInformation($"Merchant token succeeded. Merchant Token Response: {merchantTokenResponse?.Message}");

                        var transactionResponse = await PrepareStartTransactionRequest(_httpClient, url, merchantTokenResponse.data.AccessToken, walletRequest);

                    if (transactionResponse != null && transactionResponse.Message == $"{ReturnSuccess}")
                    {
                            _logger.LogInformation($"Start transaction succeeded. TransactionResponse: {transactionResponse?.Message}");

                            return new WalletResponse { Status = transactionResponse.Message };
                    }
                    else
                    {
                            _logger.LogWarning($"Start transaction failed. TransactionResponse: {transactionResponse?.Message}");
                            return new WalletResponse { Status = $"{ReturnError}" };
                    }
                  }
                    else
                    {
                        _logger.LogWarning($"Merchant token retrieval failed. MerchantTokenResponse: {merchantTokenResponse?.Message}");
                        return new WalletResponse { Status = $"{ReturnError}" };
                    }
                
            }
            catch (HttpRequestException httpEx)
            {
                // Handle HTTP request exceptions
                _logger.LogError($"HTTP request error: {httpEx.Message}");
                return new WalletResponse { Status = $"HTTP Request Error: {httpEx.Message}" };
            }
            catch (JsonException jsonEx)
            {
                // Handle JSON serialization/deserialization exceptions
                _logger.LogError($"JSON serialization/deserialization error: {jsonEx.Message}");
                return   new WalletResponse { Status = $"JSON Error: {jsonEx.Message}" };
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                _logger.LogError($"General error: {ex.Message}");
                return   new WalletResponse { Status = $"General Error: {ex.Message}" };
            }


            async Task<GetMerchantTokenResponse> CallGetMerchantToken(HttpClient client, string url, WalletRequest.OperatorCredentials credentials)
            {
                var merchantTokenRequest = new GetMerchantTokenRequest
                {
                    data = new Data
                    {
                        Username = credentials.Username,
                        Password = credentials.Password,
                        GrantType = credentials.GrantType
                    }
                };

                logging(Operator, methodAPI.GetToken, url, "Request", HttpMethod.Get, merchantTokenRequest,formattedDate);

                  var requestContent = new StringContent(JsonSerializer.Serialize(merchantTokenRequest), Encoding.UTF8, "application/json");

                var response = await SendRequest(client, HttpMethod.Get, url, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    logging(Operator, methodAPI.GetToken, url, "Response", HttpMethod.Get, response , formattedDate);
                    return await DeserializeResponse<GetMerchantTokenResponse>(response);
                }
                _logger.LogWarning($"Merchant token request failed with status code: {response.StatusCode}");
                return null;
            }


             async Task<StartTransactionResponse> PrepareStartTransactionRequest(HttpClient client, string url, string accessToken, WalletRequest walletRequest)
             {
                var startTransactionRequest = new StartTransactionRequest
                {
                    data = new DataStart
                    {
                        UserIdentifier = walletRequest.Msisdn,
                        TransactionPin = walletRequest.Credentials.MerchantPin,
                        OrderId = walletRequest.TransactionId,
                        Amount = walletRequest.Amount,
                        LanguageCode = walletRequest.LanguageCode
                    }
                };

                logging(Operator, methodAPI.Create, url, "Request", HttpMethod.Post, startTransactionRequest, formattedDate);

                var requestContent = new StringContent(JsonSerializer.Serialize(startTransactionRequest), Encoding.UTF8, "application/json");

                var headers = new Dictionary<string, string>
                        {
                            { "Authorization", $"{accessToken}" },
        
                        };

                var response = await SendRequest(client, HttpMethod.Post, url, requestContent, headers);


                if (response.IsSuccessStatusCode)
                {
                    logging(Operator, methodAPI.Create, url, "Response", HttpMethod.Post, response, formattedDate);

                    return await DeserializeResponse<StartTransactionResponse>(response);
                }
                _logger.LogWarning($"Start transaction request failed with status code: {response.StatusCode}");
                return null;
            }
        }

        async Task<HttpResponseMessage> SendRequest(HttpClient client, HttpMethod method, string url, HttpContent content , IDictionary<string, string> headers = null)
        {
            var request = new HttpRequestMessage(method, url)
            {
                Content = content
            };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            return await client.SendAsync(request);
        }

        async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent);
        }

       
        public void logging<T>(string Operator , methodAPI meth, string url, string Type, HttpMethod httpMethod  , T content  , string formattedDate)
        {
            _logger.LogInformation("Operator Name: "+ Operator +"\n API : "+ meth + "\n API URL: " + url +
                                "\n Request Or Response: " + Type + "\n Method Used: " + httpMethod +"\n Content : " + content 
                                +"\n Date: " + formattedDate 
                     
                );
        }
    }
}
