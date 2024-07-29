using System.Net;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using TrialWork.Controllers;
using TrialWork.Models;


namespace TrialWork
{
    public class Communicator
    {

        private readonly ILogger<Communicator> _logger;
        private readonly HttpClient _httpClient;
        private readonly Integration _integration;

        

        public Communicator(ILogger<Communicator> logger, HttpClient httpClient , Integration integration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _integration = integration;
              
        }

        public async Task<WalletResponse> StartTransaction(WalletRequest walletRequest )
        {



            try
            {


                var merchantTokenResponse = await CallGetMerchantToken(_httpClient , walletRequest.Credentials);

                if (merchantTokenResponse != null && merchantTokenResponse.Message == $"{Constants.ReturnSuccess}")
                {
                    _logger.LogInformation($"Merchant token succeeded. Merchant Token Response: {merchantTokenResponse?.Message}");

                    var transactionResponse = await StartTransactionRequest(_httpClient, Constants.Url, merchantTokenResponse.data.AccessToken, walletRequest);

                    if (transactionResponse != null && transactionResponse.Message == $"{Constants.ReturnSuccess}")
                    {
                        _logger.LogInformation($"Start transaction succeeded. TransactionResponse: {transactionResponse?.Message}");

                        return new WalletResponse { Status = transactionResponse.Message };
                    }
                    else
                    {
                        _logger.LogWarning($"Start transaction failed. TransactionResponse: {transactionResponse?.Message}");
                        return new WalletResponse { Status = $"{Constants.ReturnError}" };
                    }
                }
                else
                {
                    _logger.LogWarning($"Merchant token retrieval failed. MerchantTokenResponse: {merchantTokenResponse?.Message}");
                    return new WalletResponse { Status = $"{Constants.ReturnError}" };
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
                return new WalletResponse { Status = $"JSON Error: {jsonEx.Message}" };
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                _logger.LogError($"General error: {ex.Message}");
                return new WalletResponse { Status = $"General Error: {ex.Message}" };
            }
        

            async Task<GetMerchantTokenResponse> CallGetMerchantToken(HttpClient client, WalletRequest.OperatorCredentials credentials)
            {
                var requestContent = _integration.GetMerchantToken( credentials);

                var response = await SendRequest(client, HttpMethod.Get, Constants.Url, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    logging(Constants.Operator, Constants.methodAPI.GetToken, Constants.Url, "Response", HttpMethod.Get, response,  Constants.FormattedDate);
                    return await DeserializeResponse<GetMerchantTokenResponse>(response);
                }
                _logger.LogWarning($"Merchant token request failed with status code: {response.StatusCode}");
                return null;

            }

             async Task<StartTransactionResponse> StartTransactionRequest( HttpClient client,string url, string accessToken, WalletRequest walletRequest)
             {
                var (requestContent, headers) = await _integration.PrepareStartTransactionRequest( accessToken , walletRequest);

                var response = await SendRequest(client, HttpMethod.Post, url, requestContent, headers);


                if (response.IsSuccessStatusCode)
                {
                    logging(Constants.Operator, Constants.methodAPI.Create, url, "Response", HttpMethod.Post, response, Constants.FormattedDate);

                    return await DeserializeResponse<StartTransactionResponse>(response);
                }
                _logger.LogWarning($"Start transaction request failed with status code: {response.StatusCode}");
                return null;
            
             }






                  async Task<HttpResponseMessage> SendRequest(HttpClient client, HttpMethod method, string url, HttpContent content, IDictionary<string, string> headers = null)
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
          
    }
            public void logging<T>(string Operator, Constants.methodAPI meth, string url, string Type, HttpMethod httpMethod, T content, string formattedDate)
            {
                _logger.LogInformation("Operator Name: " + Operator + "\n API : " + meth + "\n API URL: " + url +
                                    "\n Request Or Response: " + Type + "\n Method Used: " + httpMethod + "\n Content : " + content
                                    + "\n Date: " + formattedDate

                    );
            }
    }
}
