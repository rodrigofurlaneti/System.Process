using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Process.Application.Clients.Jarvis;
using System.Process.Domain.ValueObjects;
using Xunit;

namespace System.Process.UnitTests.Application.Clients.Jarvis
{
    public class JarvisClientTests
    {
        private Mock<ILogger<JarvisClient>> Logger;
        private IOptions<JarvisConfig> Config;

        public JarvisClientTests()
        {
            Logger = new Mock<ILogger<JarvisClient>>();

            Config = Options.Create(new JarvisConfig
            {
                Url = "http://10.9.11.128/jarvis/"
            });
        }

        private HttpClient CreateHttpClient<TContent>(TContent content)
        {
            var formattedContent = "";
            if (typeof(TContent) != typeof(string))
            {
                formattedContent = JsonConvert.SerializeObject(content);
            }
            else
            {
                formattedContent = (string)Convert.ChangeType(content, typeof(TContent));
            }

            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                ReasonPhrase = "ReasonPhrase",
                Version = new Version(),
                RequestMessage = new HttpRequestMessage(),
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(formattedContent)
            };

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            return new HttpClient(handlerMock.Object);
        }

        [Fact(DisplayName = "Should Get Device Details Successfully")]
        public async void ShouldGetDeviceDetailsSuccessfully()
        {
            var sessionId = "2XUMSZRUV0STQJ7NABZ3YW4PAR3J7NJVTTUFFTJD5DM3VUGEP3AC86E382A2AF339FCB122D8A8F7B8E02880DE578A598D94FBDAAC1E20B3BABE8";
            var cancellationToken = new CancellationToken();

            var httpClient = CreateHttpClient("{" +
                "\"latitude\": \"-23.706879\", " +
                "\"longitude\": \"-46.539313\", " +
                "\"altitude\": \"799.602993\", " +
                "\"ipAddress\": \"fe80::9088:a0a2:9e3d:47e6\", " +
                "\"macAddress\": \"4690FA88-FF6C-48CC-A205-5B687B46C7A8\", " +
                "\"platform\": \"iOS\", " +
                "\"manufacturer\": \"\", " +
                "\"osVersion\": \"13.5.1\", " +
                "\"model\": \"iPhone 7 (GSM+CDMA)\"" +
                "}");

            var expectedResult = new DeviceDetails
            {
                IpAddress = "fe80::9088:a0a2:9e3d:47e6",
                Platform = "iOS",
                OsVersion = "13.5.1",
                Latitude = "-23.706879",
                Longitude = "-46.539313",
                Altitude = "799.602993",
                MacAddress = "4690FA88-FF6C-48CC-A205-5B687B46C7A8",
                Model = "iPhone 7 (GSM+CDMA)",
                Manufacturer = ""
            };

            var jarvisClient = new JarvisClient(Logger.Object, httpClient, Config);

            var result = jarvisClient.GetDeviceDetails(sessionId, cancellationToken);

            result.Result.Should().BeEquivalentTo(expectedResult);

        }
    }
}
