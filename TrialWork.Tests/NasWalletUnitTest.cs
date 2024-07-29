using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using TrialWork.Controllers;
using TrialWork.Models;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using System.Threading;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace TrialWork.Tests
{
    public class WalletControllerTests
    {
        bool isMocking = true;

        private readonly HttpClient _httpClient;
        private WalletController _walletController;
        private readonly Mock<HttpClient> _mockHttpClient;

        public WalletControllerTests()
        {
            if (isMocking)
            {
                _mockHttpClient = new Mock<IHttpClient>();

                MethodInfo methodInfo = typeof(WalletControllerTests).GetMethod("StartTransactionSuccess", BindingFlags.NonPublic | BindingFlags.Instance);

                // Check if the method exists and invoke it
                if (methodInfo != null)
                {
                    methodInfo.Invoke(this, null);
                }
                else
                {
                    throw new InvalidOperationException("Method 'StartTransactionSuccess' not found.");
                }

            }
            else
            {
                _httpClient = new HttpClient();
                StartTransaction();
            }
            _walletController = new WalletController(new NullLogger<WalletController>(), _httpClient);
        }

        [Fact]
        public async Task StartTransaction()
        {
            // Arrange
            var walletRequest = new WalletRequest
            {
                Msisdn = "123456789",
                Credentials = new WalletRequest.OperatorCredentials
                {
                    Username = "username",
                    Password = "password",
                    GrantType = "grant_type",
                    MerchantPin = "pin"
                },
                TransactionId = "trans_id",
                Amount = "100",
                LanguageCode = "en"
            };


            // Act
            var result = await _walletController.StartTransaction(walletRequest);

            // Assert
            Assert.Equal("Success", result.Status);
        }

        private async void SetupMockHttpResponse<T>(T response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var message = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(JsonSerializer.Serialize(response))
            };


            _mockHttpClient
                .Protected()
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>())
                
                )
                .ReturnsAsync(message)
                .Verifiable();


            //Setup(x => x.SendAsync()
            //ItExpr.IsAny<HttpRequestMessage>(),
            //        ItExpr.IsAny<CancellationToken>()))
        }


        private void StartTransactionSuccess()
        {
            var merchantTokenResponse = new GetMerchantTokenResponse
            {

                Message = "Success",
                data = new GetMerchantTokenResponse.DataGetMerchant
                {
                    AccessToken = "access_token"
                }
            };
            var transactionResponse = new StartTransactionResponse
            {
                Message = "Success"
            };

            //Mock Response
            SetupMockHttpResponse(merchantTokenResponse);
            SetupMockHttpResponse(transactionResponse);
        }
    



    }


}
    
