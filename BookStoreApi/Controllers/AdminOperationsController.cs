using BookStoreApi.Constants;
using BookStoreApi.Models.DTOs;
using BookStoreApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = nameof(Roles.Admin))]
public class AdminOperationsController : ControllerBase
{
    private readonly IUserOrderRepository _userOrderRepository;

    public AdminOperationsController(IUserOrderRepository userOrderRepository)
    {
        _userOrderRepository = userOrderRepository;
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _userOrderRepository.UserOrders(true);
        return Ok(orders);
    }

    [HttpPut("orders/{orderId:int}/toggle-payment")]
    public async Task<IActionResult> TogglePaymentStatus(int orderId)
    {
        try
        {
            await _userOrderRepository.TogglePaymentStatus(orderId);
            return Ok(new { message = "Payment status updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Failed to update payment status" });
        }
    }

    [HttpGet("orders/{orderId:int}")]
    public async Task<IActionResult> GetOrderStatus(int orderId)
    {
        var order = await _userOrderRepository.GetOrderById(orderId);
        if (order == null)
        {
            return NotFound(new { message = $"Order with id {orderId} not found" });
        }

        var statuses = await _userOrderRepository.GetOrderStatuses();

        return Ok(new
        {
            orderId = order.Id,
            currentStatusId = order.OrderStatusId,
            statuses
        });
    }

    [HttpPut("orders/status")]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusModel data)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _userOrderRepository.ChangeOrderStatus(data);
            return Ok(new { message = "Order status updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Something went wrong" });
        }
    }

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        return Ok(new { message = "Admin dashboard endpoint" });
    }
}
