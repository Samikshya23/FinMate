using System.Net.Http.Headers;

namespace FinMate.web.Services
{
    public class ApiClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _http;

        public ApiClient(IHttpClientFactory factory, IConfiguration config, IHttpContextAccessor http)
        {
            _factory = factory;
            _config = config;
            _http = http;
        }

        public HttpClient CreateAuthorizedClient()
        {
            var baseUrl = _config["ApiBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("ApiBaseUrl missing in appsettings.json");

            var client = _factory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);

            var token = _http.HttpContext?.Request.Cookies["finmate_token"];
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }
    }
}
