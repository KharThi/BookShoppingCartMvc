using BookStoreApi.Models.DTOs;
using BookStoreApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/user-orders")]
[Authorize]
public class UserOrderController : ControllerBase
{
    private readonly IUserOrderRepository _userOrderRepo;

    public UserOrderController(IUserOrderRepository userOrderRepo)
    {
        _userOrderRepo = userOrderRepo;
    }

    [HttpGet("orders/{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _userOrderRepo.GetOrderById(id);

        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpGet("order-statuses")]
    public async Task<IActionResult> GetOrderStatuses()
    {
        var statuses = await _userOrderRepo.GetOrderStatuses();
        return Ok(statuses);
    }

    [HttpPut("orders/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromBody] UpdateOrderStatusModel data)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _userOrderRepo.ChangeOrderStatus(data);
            return Ok(new { message = "Order status updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("orders/payment/{orderId}")]
    public async Task<IActionResult> TogglePaymentStatus(int orderId)
    {
        try
        {
            await _userOrderRepo.TogglePaymentStatus(orderId);
            return Ok(new { message = "Payment status updated" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _userOrderRepo.UserOrders(getAll: true);
        return Ok(orders);
    }
}