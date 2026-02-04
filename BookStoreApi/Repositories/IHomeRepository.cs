using BookStoreApi.Data;
using BookStoreApi.Models;
using BookStoreApi.Models.DTOs;
namespace BookStoreApi
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreId = 0);
        Task<IEnumerable<Genre>> Genres();
    }
}