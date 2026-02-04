using BookShoppingCartMvcUI.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace BookShoppingCartMvcUI.Controllers
{
    [AllowAnonymous]
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly VnPaymentService _vnPaymentService;

        public CartController(
            IHttpClientFactory httpClientFactory,
            VnPaymentService vnPaymentService)
        {
            _httpClientFactory = httpClientFactory;
            _vnPaymentService = vnPaymentService;
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

        public async Task<IActionResult> AddItem(int bookId, int qty = 1, int redirect = 0)
        {
            var client = CreateClient();

            var response = await client.PostAsync(
                $"api/cart/items?BookId={bookId}&Qty={qty}",
                    null);

            if (!response.IsSuccessStatusCode)
                return BadRequest();

            var cartCount = await response.Content.ReadFromJsonAsync<CartCountDto>();

            if (redirect == 0)
                return Ok(cartCount.CartItemCount);

            return RedirectToAction(nameof(GetUserCart));
        }

        public async Task<IActionResult> RemoveItem(int bookId)
        {
            var client = CreateClient();

            await client.DeleteAsync(
                $"api/cart/items/{bookId}");

            return RedirectToAction(nameof(GetUserCart));
        }

        public async Task<IActionResult> GetUserCart()
        {
            var client = CreateClient();

            var response = await client.GetAsync("api/cart");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (!response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                return View();
            }

            var cart = await response.Content.ReadFromJsonAsync<ShoppingCart>();

            return View(cart);
        }


        public async Task<IActionResult> GetTotalItemInCart()
        {
            var client = _httpClientFactory.CreateClient("BookStoreApi");

            var response = await client.GetAsync("api/cart/count");

            if (!response.IsSuccessStatusCode)
                return Ok(0);

            var result = await response.Content.ReadFromJsonAsync<CartCountDto>();

            return Ok(result?.CartItemCount ?? 0);
        }

        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = CreateClient();

            if (model.PaymentMethod != "COD")
            {
                var response = await client.PostAsJsonAsync(
                    "api/cart/Checkout",
                    model);

                if (!response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(OrderFailure));

                var paymentRequest =
                    await response.Content.ReadFromJsonAsync<Order>();

                return Redirect(_vnPaymentService.ToUrl(paymentRequest));
            }
            else
            {
                var response = await client.PostAsJsonAsync(
                    "api/cart/checkout",
                    model);

                if (!response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(OrderFailure));

                return RedirectToAction(nameof(OrderSuccess));
            }
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }

        public IActionResult OrderFailure()
        {
            return View();
        }

        public IActionResult VnPaymentResponse(VnPaymentResponse obj)
        {
            return View(obj);
        }
    }
}
