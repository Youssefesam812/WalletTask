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

namespace TrialWork.Tests
{
    public class WalletControllerTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly WalletController _walletController;

        public WalletControllerTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _walletController = new WalletController(new NullLogger<WalletController>(), _httpClient);
        }

        [Fact]
        public async Task StartTransaction_ReturnsSuccess_WhenTransactionSucceeds()
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

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(merchantTokenResponse))
                });

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Headers.Authorization != null),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(transactionResponse))
                });

            // Act
            var result = await _walletController.StartTransaction(walletRequest);

            // Assert
            Assert.Equal("Success", result.Status);
        }

        [Fact]
        public async Task StartTransaction_ReturnsFailure_WhenMerchantTokenFails()
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

            var merchantTokenResponse = new GetMerchantTokenResponse
            {
                Message = "Failure"
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(merchantTokenResponse))
                });

            // Act
            var result = await _walletController.StartTransaction(walletRequest);

            // Assert
            Assert.Equal("Failure", result.Status);
        }
    }
}
