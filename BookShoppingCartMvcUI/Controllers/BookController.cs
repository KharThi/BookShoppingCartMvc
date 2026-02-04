using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;

namespace BookShoppingCartMvcUI.Controllers;

[Authorize(Roles = nameof(Roles.Admin))]
public class BookController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BookController(IHttpClientFactory httpClientFactory)
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

    public async Task<IActionResult> Index()
    {
        var client = CreateClient();

        var response = await client.GetAsync("api/books");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Content($"API ERROR: {response.StatusCode}\n{error}");
        }

        var books = await response.Content.ReadFromJsonAsync<List<Book>>();
        return View(books);
    }

    public async Task<IActionResult> AddBook()
    {
        var client = CreateClient();

        var genres = await client
            .GetFromJsonAsync<List<Genre>>("api/books/genres");

        var model = new BookDTO
        {
            GenreList = genres.Select(g => new SelectListItem
            {
                Text = g.GenreName,
                Value = g.Id.ToString()
            })
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook(BookDTO model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = CreateClient();

        var form = new MultipartFormDataContent
        {
            { new StringContent(model.BookName), "BookName" },
            { new StringContent(model.AuthorName), "AuthorName" },
            { new StringContent(model.Price.ToString()), "Price" },
            { new StringContent(model.GenreId.ToString()), "GenreId" }
        };

        if (model.ImageFile != null)
        {
            form.Add(
                new StreamContent(model.ImageFile.OpenReadStream()),
                "ImageFile",
                model.ImageFile.FileName
            );
        }

        var response = await client.PostAsync("api/books", form);

        if (!response.IsSuccessStatusCode)
        {
            TempData["errorMessage"] = "Failed to add book";
            return View(model);
        }

        TempData["successMessage"] = "Book added successfully";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> UpdateBook(int id)
    {
        var client = CreateClient();

        var book = await client
            .GetFromJsonAsync<BookDTO>($"api/books/{id}");

        if (book == null)
        {
            TempData["errorMessage"] = "Book not found";
            return RedirectToAction(nameof(Index));
        }

        var genres = await client
            .GetFromJsonAsync<List<Genre>>("api/books/genres");

        book.GenreList = genres.Select(g => new SelectListItem
        {
            Text = g.GenreName,
            Value = g.Id.ToString(),
            Selected = g.Id == book.GenreId
        });

        return View(book);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateBook(BookDTO model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = CreateClient();

        var form = new MultipartFormDataContent
        {
            { new StringContent(model.BookName), "BookName" },
            { new StringContent(model.AuthorName), "AuthorName" },
            { new StringContent(model.Price.ToString()), "Price" },
            { new StringContent(model.GenreId.ToString()), "GenreId" }
        };

        if (model.ImageFile != null)
        {
            form.Add(
                new StreamContent(model.ImageFile.OpenReadStream()),
                "ImageFile",
                model.ImageFile.FileName
            );
        }

        var response = await client.PutAsync($"api/books/{model.Id}", form);

        TempData["successMessage"] = response.IsSuccessStatusCode
            ? "Book updated successfully"
            : "Failed to update book";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DeleteBook(int id)
    {
        var client = CreateClient();

        var response = await client.DeleteAsync($"api/books/{id}");

        TempData["successMessage"] = response.IsSuccessStatusCode
            ? "Book deleted successfully"
            : "Failed to delete book";

        return RedirectToAction(nameof(Index));
    }
}
