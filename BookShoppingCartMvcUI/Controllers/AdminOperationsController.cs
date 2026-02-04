using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;

namespace BookShoppingCartMvcUI.Controllers;

[Authorize(Roles = nameof(Roles.Admin))]
public class AdminOperationsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminOperationsController(IHttpClientFactory httpClientFactory)
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

    public async Task<IActionResult> AllOrders()
    {
        var client = CreateClient();

        var response = await client.GetAsync("api/admin/orders");
        if (!response.IsSuccessStatusCode)
            return View(new List<Order>());

        var orders = await response.Content
            .ReadFromJsonAsync<List<Order>>();

        return View(orders);
    }

    public async Task<IActionResult> TogglePaymentStatus(int orderId)
    {
        var client = CreateClient();

        await client.PutAsync($"api/admin/orders/{orderId}/toggle-payment", null);

        return RedirectToAction(nameof(AllOrders));
    }

    [HttpGet]
    public async Task<IActionResult> UpdateOrderStatus(int orderId)
    {
        var client = CreateClient();

        var order = await client.GetFromJsonAsync<Order>(
            $"api/admin/orders/{orderId}");

        var statuses = await client.GetFromJsonAsync<List<OrderStatus>>(
            "api/user-orders/order-statuses");

        var model = new UpdateOrderStatusModel
        {
            OrderId = orderId,
            OrderStatusId = order.OrderStatusId,
            OrderStatusList = statuses.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.StatusName,
                Selected = s.Id == order.OrderStatusId
            }).ToList()
        };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = CreateClient();

        var response = await client.PutAsJsonAsync(
            "api/admin/orders/status",
            new
            {
                OrderId = model.OrderId,
                OrderStatusId = model.OrderStatusId
            });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Failed to update order status");
            return View(model);
        }

        TempData["Success"] = "Order status updated successfully";

        return RedirectToAction("AllOrders");
    }

    public IActionResult Dashboard()
    {
        return View();
    }
}
