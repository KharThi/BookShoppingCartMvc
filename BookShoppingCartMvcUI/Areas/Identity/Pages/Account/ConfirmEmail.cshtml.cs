#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookShoppingCartMvcUI.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfirmEmailModel(UserManager<IdentityUser> userManager, IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
        }

        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                return RedirectToPage("/Index");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("BookStoreApi");

                var response = await client.GetAsync(
                    $"api/home/confirm-email?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}");

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Thank you for confirming your email.";
                    return Page();
                }

                var error = await response.Content.ReadAsStringAsync();
                StatusMessage = $"Error confirming email: {error}";
                return Page();
            }
            catch
            {
                StatusMessage = "Something went wrong while confirming your email.";
                return Page();
            }
        }
    }
}
