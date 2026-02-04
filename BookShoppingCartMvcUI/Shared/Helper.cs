using System.Security.Cryptography;
using System.Text;

namespace BookShoppingCartMvcUI.Shared
{
    public class Helper
    {
        public static string HmacSha512(string text, string key)
        {
            HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLower();
        }
    }
}
