using System.Net.Http.Json;
using FinMate.web.Models;

namespace FinMate.web.Services
{
    public class BudgetService
    {
        private readonly HttpClient _http;

        public BudgetService(HttpClient http)
        {
            _http = http;
        }

        // Adjust endpoint if your API route differs
        public Task<List<BudgetVm>?> GetBudgetsByMonth(string month)
            => _http.GetFromJsonAsync<List<BudgetVm>>($"api/Budget/month/{month}");

        public async Task<BudgetVm?> UpsertBudget(BudgetUpsertVm vm)
        {
            var res = await _http.PostAsJsonAsync("api/Budget", vm);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<BudgetVm>();
        }
    }
}
