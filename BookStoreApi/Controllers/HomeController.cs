using BookStoreApi.Constants;
using BookStoreApi.Models;
using BookStoreApi.Models.DTOs;
using BookStoreApi.Repositories;
using BookStoreApi.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace BookStoreApi.Controllers;

[ApiController]
[Route("api/home")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHomeRepository _homeRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenHandler _tokenHandler;
    private readonly IUserStore<IdentityUser> _userStore;
    private readonly IEmailHelper _emailHelper;

    public HomeController(
        ILogger<HomeController> logger,
        IHomeRepository homeRepository,
        UserManager<IdentityUser> userManager,
        ITokenHandler tokenHandler,
        IUserStore<IdentityUser> userStore,
        IEmailHelper emailHelper)
    {
        _logger = logger;
        _homeRepository = homeRepository;
        _userManager = userManager;
        _tokenHandler = tokenHandler;
        _userStore = userStore;
        _emailHelper = emailHelper;
    }

    [HttpGet("books")]
    public async Task<IActionResult> GetBooks(
        [FromQuery] string sterm = "",
        [FromQuery] int genreId = 0)
    {
        try
        {
            var books = await _homeRepository.GetBooks(sterm, genreId);
            var genres = await _homeRepository.Genres();

            var response = new BookDisplayModel
            {
                Books = books,
                Genres = genres,
                STerm = sterm,
                GenreId = genreId
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching books");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres()
    {
        var genres = await _homeRepository.Genres();
        return Ok(genres);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromQuery] string email, string password)
    {        
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Unauthorized();
        }

        var isExit = await _userManager.CheckPasswordAsync(user, password);

        if (isExit == false)
        {
            return Unauthorized();
        }
        if (!user.EmailConfirmed)
        {
            return BadRequest("Your account is inactive");
        }
        string accessToken = await _tokenHandler.CreateAccessToken(user);


        return Ok(new LoginResult { Successful = true, AccessToken = accessToken});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromQuery] string Email, string Password, string ClientUrl)
    {
        var user = new IdentityUser
        {
            UserName = Email,
            Email = Email
        };

        var result = await _userManager.CreateAsync(user, Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, Roles.User.ToString());

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var confirmUrl =
            $"{ClientUrl}/Identity/Account/ConfirmEmail?userId={user.Id}&code={encodedToken}";

        await _emailHelper.SendEmailAsync(new EmailRequest
        {
            To = user.Email,
            Subject = "Confirm your email",
            Content = $"Click <a href='{confirmUrl}'>here</a> to confirm your email"
        });

        return Ok(new { message = "Registration successful" });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return BadRequest("Invalid user");

        var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

        var result = await _userManager.ConfirmEmailAsync(user, decodedCode);

        if (!result.Succeeded)
            return BadRequest("Email confirmation failed");

        return Ok(new { message = "Email confirmed successfully" });
    }

}
