using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize]
    public class UserOrderController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserOrderController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("BookStoreApi");

            var token = Request.Cookies["JWToken"];
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<IActionResult> UserOrders()
        {
            var client = CreateClient();

            var response = await client.GetAsync("api/user-orders");

            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = "Cannot load orders";
                return View(new List<Order>());
            }

            var orders = await response.Content
                .ReadFromJsonAsync<List<Order>>();

            return View(orders);
        }
    }
}
