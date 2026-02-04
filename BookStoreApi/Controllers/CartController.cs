using BookStoreApi.Models.DTOs;
using BookStoreApi.Repositories;
using BookStoreApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartRepository _cartRepo;
    private readonly VnPaymentService _vnPaymentService;

    public CartController(
        ICartRepository cartRepo,
        VnPaymentService vnPaymentService)
    {
        _cartRepo = cartRepo;
        _vnPaymentService = vnPaymentService;
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromQuery] int bookId, [FromQuery] int qty = 1)
    {
        var email = User.FindFirstValue(ClaimTypes.Name);
        var cartCount = await _cartRepo.AddItem(bookId, qty, email);
        int count = await _cartRepo.GetCartItemCount(email);
        return Ok(new { cartItemCount = count });
    }

    [HttpDelete("items/{bookId:int}")]
    public async Task<IActionResult> RemoveItem(int bookId)
    {
        var email = User.FindFirstValue(ClaimTypes.Name);
        var cartCount = await _cartRepo.RemoveItem(bookId, email);
        int count = await _cartRepo.GetCartItemCount(email);
        return Ok(new { cartItemCount = count });
    }

    [HttpGet]
    public async Task<IActionResult> GetUserCart()
    {
        var email = User.FindFirstValue(ClaimTypes.Name);
        var cart = await _cartRepo.GetUserCart(email);
        return Ok(cart);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetTotalItemInCart()
    {
        var email = User.FindFirstValue(ClaimTypes.Name);
        int count = await _cartRepo.GetCartItemCount(email);
        return Ok(new { cartItemCount = count });
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (model.PaymentMethod != "COD")
        {
            var paymentInfo = await _cartRepo.DoCheckoutVNPay(model);
            if (paymentInfo == null)
                return BadRequest(new { message = "Checkout failed" });
            return Ok(paymentInfo);
        }

        bool isCheckedOut = await _cartRepo.DoCheckout(model);
        if (!isCheckedOut)
            return BadRequest(new { message = "Checkout failed" });

        return Ok(new { message = "Order placed successfully" });
    }

    [AllowAnonymous]
    [HttpGet("vnpay-response")]
    public IActionResult VnPaymentResponse([FromQuery] VnPaymentResponse response)
    {
        return Ok(response);
    }
}
