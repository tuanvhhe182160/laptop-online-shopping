using System.Text;
using System.Text.RegularExpressions;

namespace WebAPI.Services
{
    public class PromptSecurityService
    {
        // 1. Danh sách từ khóa Tiếng Anh & Tiếng Việt mở rộng
        private static readonly string[] DangerousKeywords =
        {
            // English injection patterns
            "ignore previous", "ignore all instructions", "system prompt", "developer message",
            "reveal prompt", "jailbreak", "bypass", "forget your rules", "forget previous",
            "pretend you are", "act as", "you are now", "new instructions", "override rules",
            "do anything now", "dan mode", "developer mode", "repeat system", "print system",
            
            // Vietnamese injection patterns
            "bo qua quy tac", "bo qua lenh", "quen lenh cu", "quen quy tac",
            "hien thi system prompt", "tiet lo system prompt", "gia lam", "nhap vai",
            "thay doi vai tro", "huong dan he thong", "xem prompt"
        };

        // 2. Các mẫu Regex nâng cao phát hiện kỹ thuật lách chữ và cấu trúc độc hại
        private static readonly Regex[] DangerousRegexPatterns =
        {
            // Bắt các từ bị chèn khoảng trắng/ký tự đặc biệt, ví dụ: "i g n o r e", "j-a-i-l-b-r-e-a-k"
            new Regex(@"i\s*g\s*n\s*o\s*r\s*e", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"j\s*a\s*i\s*l\s*b\s*r\s*e\s*a\s*k", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"s\s*y\s*s\s*t\s*e\s*m", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            // Phát hiện giả lập vai trò hệ thống hoặc cấu trúc lệnh kết thúc JSON
            new Regex(@"(system|developer|admin|user)\s*:\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"\]\s*;\s*\[", RegexOptions.Compiled), // Cố tình ngắt chuỗi JSON
            
            // Bắt các hành vi cố tình yêu cầu bỏ qua/thay đổi quy tắc
            new Regex(@"(disregard|forget|override|ignore)\s+.*(instruction|rule|prompt|system)", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        };

        public bool ContainsInjection(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string normalized = NormalizeText(input);

            foreach (var keyword in DangerousKeywords)
            {
                if (normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            foreach (var regex in DangerousRegexPatterns)
            {
                if (regex.IsMatch(input) || regex.IsMatch(normalized))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Loại bỏ dấu tiếng Việt, loại bỏ ký tự đặc biệt trùng lặp để đơn giản hóa việc quét
        /// </summary>
        private static string NormalizeText(string text)
        {
            text = text.ToLowerInvariant();

            // Loại bỏ dấu tiếng Việt (ví dụ: "bỏ qua" -> "bo qua")
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Thay thế nhiều khoảng trắng hoặc ký tự phân cách liền nhau bằng 1 khoảng trắng
            result = Regex.Replace(result, @"[^\w\s]", " "); // Xóa dấu câu: .,-!@#$%^&*
            result = Regex.Replace(result, @"\s+", " ");    // Gom nhiều khoảng trắng thành 1

            return result.Trim();
        }
    }
}