using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class GenreController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GenreController(IHttpClientFactory httpClientFactory)
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

            var genres = await client.GetFromJsonAsync<IEnumerable<Genre>>(
                "api/genres");

            return View(genres);
        }

        public IActionResult AddGenre()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddGenre(GenreDTO genre)
        {
            if (!ModelState.IsValid)
                return View(genre);

            try
            {
                var client = CreateClient();

                var response = await client.PostAsJsonAsync(
                    "api/genres",
                    genre);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["errorMessage"] = "Genre could not be added!";
                    return View(genre);
                }

                TempData["successMessage"] = "Genre added successfully";
                return RedirectToAction(nameof(AddGenre));
            }
            catch
            {
                TempData["errorMessage"] = "Genre could not be added!";
                return View(genre);
            }
        }

        public async Task<IActionResult> UpdateGenre(int id)
        {
            var client = CreateClient();

            var genre = await client.GetFromJsonAsync<GenreDTO>(
                $"api/genres/{id}");

            if (genre == null)
                throw new InvalidOperationException($"Genre with id: {id} does not found");

            return View(genre);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGenre(GenreDTO genreToUpdate)
        {
            if (!ModelState.IsValid)
                return View(genreToUpdate);

            try
            {
                var client = CreateClient();

                var response = await client.PutAsJsonAsync(
                            $"api/genres/{genreToUpdate.Id}",
                            genreToUpdate);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["errorMessage"] = "Genre could not updated!";
                    return View(genreToUpdate);
                }

                TempData["successMessage"] = "Genre is updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["errorMessage"] = "Genre could not updated!";
                return View(genreToUpdate);
            }
        }

        public async Task<IActionResult> DeleteGenre(int id)
        {
            var client = CreateClient();

            var response = await client.DeleteAsync(
                $"api/genres/{id}");

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Genre with id: {id} does not found");

            return RedirectToAction(nameof(Index));
        }
    }
}
