// Bọc tất cả vào sự kiện DOMContentLoaded để tạo Scope cục bộ
document.addEventListener("DOMContentLoaded", () => {

    const API_BASE = document.body.getAttribute('data-api-base') || '';
    const TOKEN = document.body.getAttribute('data-token');

    // Hàm lấy Header chứa Token
    function getHeaders() {
        const headers = { 'Content-Type': 'application/json' };
        if (TOKEN) headers['Authorization'] = `Bearer ${TOKEN}`;
        return headers;
    }

    const form = document.getElementById("checkoutForm");
    if (form) {
        form.addEventListener("submit", async (e) => {
            // Chặn reload trang
            e.preventDefault();

            // Kiểm tra trạng thái đăng nhập
            if (!TOKEN) {
                if (confirm('Phiên đăng nhập đã hết hạn hoặc chưa đăng nhập. Chuyển đến trang đăng nhập?')) {
                    window.location.href = '/Auth/CustomerLogin';
                }
                return;
            }

            const isBuyNow = document.getElementById("isBuyNow").value === "true";

            // LƯU Ý CHO HTML: Đảm bảo input hidden trong file Razor/HTML của bạn đã đổi id thành "variantId"
            const variantIdInput = document.getElementById("variantId");
            const variantId = variantIdInput ? parseInt(variantIdInput.value) : 0;

            let quantity = parseInt(document.getElementById("quantity")?.value || 0);

            if (isBuyNow) {
                const qElem = document.getElementById('buyNowQuantity');
                if (qElem) quantity = parseInt(qElem.value) || 1;
            }

            const shippingAddress = document.getElementById("shippingAddress")?.value || "";
            const paymentMethod = document.getElementById("paymentMethod")?.value || "COD";

            // Hiển thị loading (tùy chọn, dùng SweetAlert2)
            Swal.fire({
                title: 'Đang xử lý giao dịch...',
                text: 'Vui lòng không đóng trình duyệt.',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Gửi Payload tương ứng với logic Mua ngay (Direct) hoặc Mua từ Giỏ (Cart)
            if (isBuyNow) {
                const payload = { variantId, quantity, shippingAddress, paymentMethod };
                await processCheckout(`${API_BASE}/api/Orders/checkout-direct`, payload, paymentMethod);
            } else {
                const payload = { shippingAddress, paymentMethod };
                await processCheckout(`${API_BASE}/api/Orders/checkout-cart`, payload, paymentMethod);
            }
        });
    }

    // Xử lý gửi Request và phản hồi UI
    async function processCheckout(url, payload, paymentMethod) {
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: getHeaders(),
                body: JSON.stringify(payload)
            });

            if (response.status === 401 || response.status === 403) {
                Swal.fire({ title: 'Lỗi quyền truy cập', text: 'Vui lòng đăng nhập lại với tài khoản khách hàng.', icon: 'error' });
                return;
            }

            if (response.ok) {
                let responseJson = null;
                try {
                    responseJson = await response.json();
                } catch { }

                // Kiểm tra xem Backend có trả về paymentUrl (dùng cho VNPay) hay không
                const paymentUrl = responseJson && (responseJson.paymentUrl || responseJson.PaymentUrl);

                if (paymentMethod === 'VNPAY' && paymentUrl) {
                    // Nếu là VNPay và có link, tiến hành chuyển hướng sang cổng thanh toán VNPay
                    Swal.fire({
                        title: 'Đang chuyển hướng...',
                        text: 'Đang kết nối đến cổng thanh toán VNPay.',
                        icon: 'info',
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        window.location.href = paymentUrl;
                    });
                } else {
                    // Trường hợp COD hoặc các phương thức thông thường
                    let orderId = responseJson && (responseJson.orderId || responseJson.id || responseJson.orderID);

                    Swal.fire({
                        title: 'Đặt hàng thành công!',
                        text: orderId ? `Mã đơn hàng của bạn: #${orderId}` : 'Đơn hàng đã được ghi nhận.',
                        icon: 'success',
                        timer: 2500,
                        showConfirmButton: false
                    }).then(() => {
                        // Chuyển hướng về trang lịch sử mua hàng
                        window.location.href = "/Storefront/order-history";
                    });
                }
            } else {
                // Đọc lỗi trả về từ Backend (Ví dụ lỗi: Hết hàng, tranh chấp kho)
                let errorMsg = "Đã xảy ra lỗi khi thanh toán.";
                try {
                    const errorJson = await response.json();
                    errorMsg = errorJson.message || errorJson.title || JSON.stringify(errorJson);
                } catch {
                    errorMsg = await response.text();
                }

                Swal.fire({ title: 'Giao dịch thất bại', text: errorMsg, icon: 'error' });
            }
        } catch (e) {
            console.error(e);
            Swal.fire({ title: 'Lỗi hệ thống', text: 'Mất kết nối tới máy chủ. Vui lòng thử lại sau.', icon: 'error' });
        }
    }
});