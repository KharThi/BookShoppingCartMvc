using BookStoreApi.Constants;
using BookStoreApi.Models;
using BookStoreApi.Models.DTOs;
using BookStoreApi.Repositories;
using BookStoreApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/books")]
[Authorize(Roles = nameof(Roles.Admin))]
public class BookController : ControllerBase
{
    private readonly IBookRepository _bookRepo;
    private readonly IGenreRepository _genreRepo;
    private readonly IFileService _fileService;

    public BookController(
        IBookRepository bookRepo,
        IGenreRepository genreRepo,
        IFileService fileService)
    {
        _bookRepo = bookRepo;
        _genreRepo = genreRepo;
        _fileService = fileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _bookRepo.GetBooks();
        return Ok(books);
    }

    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres()
    {
        var genres = await _genreRepo.GetGenres();
        return Ok(genres);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBookById(int id)
    {
        var book = await _bookRepo.GetBookById(id);
        if (book == null)
            return NotFound(new { message = $"Book with id {id} not found" });

        return Ok(book);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddBook([FromForm] BookDTO bookDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            if (bookDto.ImageFile != null)
            {
                ValidateImage(bookDto.ImageFile);
                bookDto.Image = await SaveImage(bookDto.ImageFile);
                if (bookDto.Image.Contains("/images/"))
                {
                    bookDto.Image = bookDto.Image.Replace("/images/","");
                }
            }

            Book book = MapToBook(bookDto);
            await _bookRepo.AddBook(book);

            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateBook(int id, [FromForm] BookDTO bookDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingBook = await _bookRepo.GetBookById(id);
        if (existingBook == null)
            return NotFound(new { message = $"Book with id {id} not found" });

        try
        {
            string oldImage = existingBook.Image;

            if (bookDto.ImageFile != null)
            {
                ValidateImage(bookDto.ImageFile);
                existingBook.Image = await SaveImage(bookDto.ImageFile);
            }
            else
            {
                existingBook.Image = oldImage;
            }

            existingBook.BookName = bookDto.BookName;
            existingBook.Price = bookDto.Price;
            existingBook.GenreId = bookDto.GenreId;
            existingBook.AuthorName = bookDto.AuthorName;

            await _bookRepo.UpdateBook(existingBook);

            if (!string.IsNullOrWhiteSpace(oldImage) && oldImage != existingBook.Image)
            {
                _fileService.DeleteFile(oldImage);
            }

            return Ok(existingBook);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _bookRepo.GetBookById(id);
        if (book == null)
            return NotFound(new { message = $"Book with id {id} not found" });

        try
        {
            await _bookRepo.DeleteBook(book);

            if (!string.IsNullOrWhiteSpace(book.Image))
            {
                _fileService.DeleteFile(book.Image);
            }

            return Ok(new { message = "Book deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    private static void ValidateImage(IFormFile file)
    {
        if (file.Length > 1 * 1024 * 1024)
            throw new InvalidOperationException("Image file cannot exceed 1 MB");

        string[] allowedExtensions = [".jpeg", ".jpg", ".png"];
        var ext = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(ext))
            throw new InvalidOperationException("Invalid image format");
    }

    private async Task<string> SaveImage(IFormFile file)
    {
        string[] allowedExtensions = [".jpeg", ".jpg", ".png"];
        return await _fileService.SaveFile(file, allowedExtensions);
    }

    private static Book MapToBook(BookDTO dto, int? id = null)
    {
        return new Book
        {
            Id = id ?? dto.Id,
            BookName = dto.BookName,
            AuthorName = dto.AuthorName,
            GenreId = dto.GenreId,
            Price = dto.Price,
            Image = dto.Image
        };
    }
}
