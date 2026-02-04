using BookStoreApi.Data;
using BookStoreApi.Models;
using BookStoreApi.Models.DTOs;
namespace BookStoreApi.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddItem(int bookId, int qty, string email);
        Task<int> RemoveItem(int bookId, string email);
        Task<ShoppingCart> GetUserCart(string email);
        Task<int> GetCartItemCount(string email, string userId = "");
        Task<ShoppingCart> GetCart(string userId);
        Task<bool> DoCheckout(CheckoutModel model);
        Task<Order> DoCheckoutVNPay(CheckoutModel model);

	}
}
