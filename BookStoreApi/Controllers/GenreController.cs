using BookStoreApi.Constants;
using BookStoreApi.Models;
using BookStoreApi.Models.DTOs;
using BookStoreApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/genres")]
[Authorize(Roles = nameof(Roles.Admin))]
public class GenreController : ControllerBase
{
    private readonly IGenreRepository _genreRepo;

    public GenreController(IGenreRepository genreRepo)
    {
        _genreRepo = genreRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetGenres()
    {
        var genres = await _genreRepo.GetGenres();
        return Ok(genres);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGenreById(int id)
    {
        var genre = await _genreRepo.GetGenreById(id);
        if (genre == null)
            return NotFound(new { message = $"Genre with id {id} not found" });

        return Ok(genre);
    }

    [HttpPost]
    public async Task<IActionResult> AddGenre([FromBody] GenreDTO genreDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var genre = new Genre
            {
                Id = genreDto.Id,
                GenreName = genreDto.GenreName
            };

            await _genreRepo.AddGenre(genre);

            return CreatedAtAction(nameof(GetGenreById), new { id = genre.Id }, genre);
        }
        catch
        {
            return BadRequest(new { message = "Genre could not be added" });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateGenre(int id, [FromBody] GenreDTO genreDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _genreRepo.GetGenreById(id);
        if (existing == null)
            return NotFound(new { message = $"Genre with id {id} not found" });

        try
        {
            existing.GenreName = genreDto.GenreName;
            await _genreRepo.UpdateGenre(existing);

            return Ok(existing);
        }
        catch
        {
            return BadRequest(new { message = "Genre could not be updated" });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        var genre = await _genreRepo.GetGenreById(id);
        if (genre == null)
            return NotFound(new { message = $"Genre with id {id} not found" });

        try
        {
            await _genreRepo.DeleteGenre(genre);
            return NoContent();
        }
        catch
        {
            return BadRequest(new { message = "Genre could not be deleted" });
        }
    }
}