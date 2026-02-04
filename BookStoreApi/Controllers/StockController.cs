using BookStoreApi.Constants;
using BookStoreApi.Models.DTOs;
using BookStoreApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/stocks")]
[Authorize(Roles = nameof(Roles.Admin))]
public class StockController : ControllerBase
{
    private readonly IStockRepository _stockRepo;

    public StockController(IStockRepository stockRepo)
    {
        _stockRepo = stockRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetStocks([FromQuery] string sTerm = "")
    {
        var stocks = await _stockRepo.GetStocks(sTerm);
        return Ok(stocks);
    }

    [HttpGet("book/{bookId:int}")]
    public async Task<IActionResult> GetStockByBookId(int bookId)
    {
        var existingStock = await _stockRepo.GetStockByBookId(bookId);

        var stock = new StockDTO
        {
            BookId = bookId,
            Quantity = existingStock?.Quantity ?? 0
        };

        return Ok(stock);
    }

    [HttpPost]
    public async Task<IActionResult> ManageStock([FromBody] StockDTO stock)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _stockRepo.ManageStock(stock);
            return Ok(new
            {
                message = "Stock updated successfully"
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                message = "Something went wrong while updating stock"
            });
        }
    }
}