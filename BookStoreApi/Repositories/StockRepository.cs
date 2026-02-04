using BookStoreApi.Data;
using BookStoreApi.Models;
using BookStoreApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDbContext _context;

        public StockRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Stock?> GetStockByBookId(int bookId) => await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == bookId);

        public async Task ManageStock(StockDTO stockToManage)
        {
            var existingStock = await GetStockByBookId(stockToManage.BookId);
            if (existingStock is null)
            {
                var stock = new Stock { BookId = stockToManage.BookId, Quantity = stockToManage.Quantity };
                _context.Stocks.Add(stock);
            }
            else
            {
                existingStock.Quantity = stockToManage.Quantity;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "")
        {
            var stocks = await (from book in _context.Books
                                join stock in _context.Stocks
                                on book.Id equals stock.BookId
                                into book_stock
                                from bookStock in book_stock.DefaultIfEmpty()
                                where string.IsNullOrWhiteSpace(sTerm) || book.BookName.ToLower().Contains(sTerm.ToLower())
                                select new StockDisplayModel
                                {
                                    BookId = book.Id,
                                    BookName = book.BookName,
                                    Quantity = bookStock == null ? 0 : bookStock.Quantity
                                }
                                ).ToListAsync();
            return stocks;
        }

        public async Task AddStock(StockDTO stock)
        {
            var exists = await GetStockByBookId(stock.BookId);
            if (exists != null)
                throw new InvalidOperationException("Stock already exists for this book.");

            var entity = new Stock
            {
                BookId = stock.BookId,
                Quantity = stock.Quantity
            };

            _context.Stocks.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStock(StockDTO stock)
        {
            var existing = await GetStockByBookId(stock.BookId);
            if (existing == null)
                throw new KeyNotFoundException("Stock not found.");

            existing.Quantity = stock.Quantity;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStock(int bookId)
        {
            var stock = await GetStockByBookId(bookId);
            if (stock == null)
                throw new KeyNotFoundException("Stock not found.");

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
        }


    }

    public interface IStockRepository
    {
        Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "");
        Task<Stock?> GetStockByBookId(int bookId);
        Task ManageStock(StockDTO stockToManage);
        Task AddStock(StockDTO stock);
        Task UpdateStock(StockDTO stock);
        Task DeleteStock(int bookId);
    }
}
