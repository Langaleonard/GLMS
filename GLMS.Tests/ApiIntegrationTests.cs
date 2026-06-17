using System.Net;

namespace GLMS.Tests
{
    public class ApiIntegrationTests
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5155/")
            };
        }

        [Fact]
        public async Task GetClients_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("api/Clients");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetContracts_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("api/Contracts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetServiceRequests_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("api/ServiceRequests");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Swagger_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("swagger/index.html");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}