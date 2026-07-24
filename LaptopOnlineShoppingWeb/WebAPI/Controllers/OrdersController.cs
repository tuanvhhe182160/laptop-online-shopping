using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Security.Claims;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;
        private readonly IConfiguration _configuration;

        public OrdersController(IOrderService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        private int GetCurrentCustomerId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdStr!);
        }

        [HttpGet]
        [EnableQuery]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _service.GetAllOrdersAsync();

            if (User.IsInRole("Staff"))
            {
                var branchIdClaim = User.FindFirst("BranchId")?.Value;
                if (!string.IsNullOrEmpty(branchIdClaim) && int.TryParse(branchIdClaim, out int branchId))
                {
                    orders = orders.Where(o => o.BranchId == branchId);
                }
                else
                {
                    return Forbid("Tài khoản Staff của bạn chưa được gắn với Chi nhánh nào.");
                }
            }

            return Ok(orders.AsQueryable());
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrders()
        {
            var orders = await _service.GetOrdersByCustomerAsync(GetCurrentCustomerId());
            return Ok(orders);
        }

        [HttpGet("{id}/details")]
        [Authorize]
        public async Task<IActionResult> GetDetails(int id)
        {
            var order = await _service.GetOrderDetailsAsync(id);
            if (order == null) return NotFound("Không tìm thấy đơn hàng.");

            if (User.IsInRole("Customer") && order.CustomerId != GetCurrentCustomerId())
            {
                return Forbid("Bạn không có quyền xem đơn hàng này.");
            }

            return Ok(order);
        }

        [HttpPost("checkout-cart")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CheckoutFromCart([FromBody] CheckoutFromCartRequestDTO dto)
        {
            try
            {
                var order = await _service.CheckoutFromCartAsync(GetCurrentCustomerId(), dto);
                if (order == null) return BadRequest("Thanh toán thất bại. Giỏ hàng trống hoặc dữ liệu không hợp lệ.");

                string paymentMethod = dto.PaymentMethod?.ToUpper() ?? "COD";

                if (paymentMethod == "COD")
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Đặt hàng thành công với COD!",
                        OrderData = order,
                        PaymentUrl = ""
                    });
                }
                else if (paymentMethod == "VNPAY")
                {
                    string vnpayUrl = GenerateVnPayUrl(order.OrderId, order.TotalAmount);

                    return Ok(new
                    {
                        Success = true,
                        Message = "Đang chuyển hướng sang VNPay...",
                        OrderData = order,
                        PaymentUrl = vnpayUrl
                    });
                }

                return BadRequest("Phương thức thanh toán không hợp lệ.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("checkout-direct")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CheckoutDirectly([FromBody] DirectCheckoutRequestDTO dto)
        {
            try
            {
                var order = await _service.CheckoutDirectlyAsync(GetCurrentCustomerId(), dto);
                if (order == null) return BadRequest("Thanh toán thất bại. Dữ liệu không hợp lệ.");

                string paymentMethod = dto.PaymentMethod?.ToUpper() ?? "COD";

                if (paymentMethod == "COD")
                {
                    return Ok(new { Success = true, Message = "Đặt hàng thành công với COD!", OrderData = order, PaymentUrl = "" });
                }
                else if (paymentMethod == "VNPAY")
                {
                    string vnpayUrl = GenerateVnPayUrl(order.OrderId, order.TotalAmount);
                    return Ok(new { Success = true, Message = "Đang chuyển sang VNPay...", OrderData = order, PaymentUrl = vnpayUrl });
                }

                return BadRequest("Phương thức thanh toán không hợp lệ.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDTO dto)
        {
            var result = await _service.UpdateOrderStatusAsync(id, dto);
            if (!result) return NotFound("Không tìm thấy đơn hàng để cập nhật.");

            return NoContent();
        }

        private string GenerateVnPayUrl(int orderId, decimal totalAmount)
        {
            string vnp_Returnurl = _configuration["VnPay:ReturnUrl"]!;
            string vnp_Url = _configuration["VnPay:BaseUrl"]!;
            string vnp_TmnCode = _configuration["VnPay:TmnCode"]!;
            string vnp_HashSecret = _configuration["VnPay:HashSecret"]!;

            // --- CHÈN ĐOẠN NÀY ĐỂ KIỂM TRA TRỰC QUAN ---
            System.Diagnostics.Debug.WriteLine("================ VNPAY CONFIG CHECK ================");
            System.Diagnostics.Debug.WriteLine($"TmnCode đang dùng: [{vnp_TmnCode}]");
            System.Diagnostics.Debug.WriteLine($"HashSecret đang dùng: [{vnp_HashSecret}]");
            System.Diagnostics.Debug.WriteLine($"ReturnUrl đang dùng: [{vnp_Returnurl}]");
            System.Diagnostics.Debug.WriteLine("====================================================");
            // -------------------------------------------

            var vnpay = new WebAPI.Helpers.VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);

            long vnpayAmount = (long)(totalAmount * 100);
            vnpay.AddRequestData("vnp_Amount", vnpayAmount.ToString());

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (ipAddress.Contains("::"))
            {
                ipAddress = "127.0.0.1";
            }
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {orderId}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", orderId.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            System.Diagnostics.Debug.WriteLine("Generated VNPay URL: " + paymentUrl);

            return paymentUrl;
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> PaymentCallback()
        {
            var collections = HttpContext.Request.Query;
            var vnpay = new WebAPI.Helpers.VnPayLibrary();

            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    if (key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                    {
                        vnpay.AddResponseData(key, value.ToString());
                    }
                }
            }

            string vnp_HashSecret = _configuration["VnPay:HashSecret"]!;
            string vnp_SecureHash = collections.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value.ToString();

            long orderId = 0;
            long.TryParse(vnpay.GetResponseData("vnp_TxnRef"), out orderId);
            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

            if (checkSignature)
            {
                if (vnp_ResponseCode == "00")
                {
                    // Thanh toán thành công -> Chuyển hướng về Front-end kèm trạng thái success
                    return Redirect($"https://localhost:7137/Storefront/CheckoutResult?status=success&orderId={orderId}");
                }
                else
                {
                    // Thanh toán thất bại/hủy -> Chuyển hướng kèm trạng thái failed
                    return Redirect($"https://localhost:7137/Storefront/CheckoutResult?status=failed&orderId={orderId}");
                }
            }

            return BadRequest(new { success = false, message = "Lỗi xác thực chữ ký VNPay (Signature Mismatch)!" });
        }
    }
}