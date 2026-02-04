using Microsoft.Extensions.Options;
using System.Net;

namespace BookShoppingCartMvcUI.Shared
{
    public class VnPaymentService
    {
        VnPayment payment;
        IHttpContextAccessor accessor;
        public VnPaymentService(IOptions<VnPayment> options, IHttpContextAccessor accessor)
        {
            payment = options.Value;
            this.accessor = accessor;
        }
        public string ToUrl(Order obj)
        {
            var total = obj.OrderDetail.DistinctBy(x => x.Id).Sum(od => od.Quantity * od.UnitPrice) * 100;
            var totalQuantity = obj.OrderDetail.Sum(od => od.Quantity);
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>{
            {"vnp_Amount", (total).ToString()},
            {"vnp_Command", payment.Command},
            {"vnp_CreateDate", obj.CreateDate.ToString("yyyyMMddHHmmss")},
            {"vnp_CurrCode", payment.CurrCode},
            {"vnp_IpAddr", accessor.HttpContext!.Connection.RemoteIpAddress!.ToString()},
            {"vnp_Locale", payment.Locale},
            {"vnp_OrderInfo", $"Payment for {obj.Id} with amount {totalQuantity.ToString()}"},
            {"vnp_OrderType", payment.OrderType},
            {"vnp_ReturnUrl", payment.ReturnUrl},
            {"vnp_TmnCode", payment.TmnCode},
            {"vnp_TxnRef", obj.Id.ToString()},
            {"vnp_Version", payment.Version},
        };
            string query = string.Join("&", dict.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));
            string secureHash = Helper.HmacSha512(query, payment.HashSecret);
            return $"{payment.BaseUrl}?{query}&vnp_SecureHash={secureHash}";
        }
    }
}
