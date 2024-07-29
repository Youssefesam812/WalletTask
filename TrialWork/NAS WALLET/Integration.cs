using System.Text.Json;
using System.Text;
using TrialWork.Controllers;
using static TrialWork.Models.StartTransactionRequest;
using TrialWork.Models;
using static TrialWork.Models.GetMerchantTokenRequest;

namespace TrialWork
{
    public class Integration
    {



        private readonly ILogger<Integration> _logger;


      

        public Integration(ILogger<Integration> logger, HttpClient httpClient)
        {
            _logger = logger;
          
        }




        public HttpContent GetMerchantToken(  WalletRequest.OperatorCredentials credentials)
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

            logging(Constants.Operator, Constants.methodAPI.GetToken, Constants.Url, "Request", HttpMethod.Get, merchantTokenRequest, Constants.FormattedDate);

            var requestContent = new StringContent(JsonSerializer.Serialize(merchantTokenRequest), Encoding.UTF8, "application/json");

            return requestContent;
        }



        public async Task<(HttpContent content, Dictionary<string, string> headers)> PrepareStartTransactionRequest(  string accessToken, WalletRequest walletRequest)
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


             logging(Constants.Operator, Constants.methodAPI.Create, Constants.Url, "Request", HttpMethod.Post, startTransactionRequest, Constants.FormattedDate);

            var requestContent = new StringContent(JsonSerializer.Serialize(startTransactionRequest), Encoding.UTF8, "application/json");

            var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {accessToken}" }
        };

            return (requestContent, headers);
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

