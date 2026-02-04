using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class StockController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StockController(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Index(string sTerm = "")
        {
            var client = CreateClient();

            var stocks = await client.GetFromJsonAsync<List<StockDisplayModel>>(
                $"api/stocks?sTerm={sTerm}");

            return View(stocks);
        }

        public async Task<IActionResult> ManageStock(int bookId)
        {
            var client = CreateClient();

            var stock = await client.GetFromJsonAsync<StockDTO>(
                $"api/stocks/book/{bookId}");

            return View(stock);
        }

        [HttpPost]
        public async Task<IActionResult> ManageStock(StockDTO stock)
        {
            if (!ModelState.IsValid)
                return View(stock);

            var client = CreateClient();

            var response = await client.PostAsJsonAsync(
                "api/stocks", stock);

            if (!response.IsSuccessStatusCode)
            {
                TempData["errorMessage"] = "Failed to update stock";
                return View(stock);
            }

            TempData["successMessage"] = "Stock is updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManangeStock(int bookId)
        {
            var client = CreateClient();
            var existingStock = await client.GetFromJsonAsync<StockDTO>(
                $"api/stocks/book/{bookId}");
            var stock = new StockDTO
            {
                BookId = bookId,
                Quantity = existingStock != null
            ? existingStock.Quantity : 0
            };
            return View(stock);
        }

        [HttpPost]
        public async Task<IActionResult> ManangeStock(StockDTO stock)
        {
            var client = CreateClient();
            if (!ModelState.IsValid)
                return View(stock);
            try
            {
                var existingStock = await client.GetFromJsonAsync<StockDTO>(
                $"api/stocks/book/{stock.BookId}");
                var response = await client.PostAsJsonAsync("api/stocks", stock);
                TempData["successMessage"] = "Stock is updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = "Something went wrong!!";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
