const API_BASE = "https://localhost:7196/api";
const customerId = 1;

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("checkoutForm");
    if (form) {
        form.addEventListener("submit", async (e) => {
            e.preventDefault();

            const isBuyNow = document.getElementById("isBuyNow").value === "true";
            const laptopId = parseInt(document.getElementById("laptopId").value);
            const quantity = parseInt(document.getElementById("quantity").value);
            const shippingAddress = document.getElementById("shippingAddress").value;
            const paymentMethod = document.getElementById("paymentMethod").value;

            if (isBuyNow) {
                // Checkout trực tiếp
                const payload = { laptopId, quantity, shippingAddress, paymentMethod };
                await processCheckout(`${API_BASE}/orders/checkout-direct/${customerId}`, payload);
            } else {
                // Checkout từ giỏ hàng
                const payload = { shippingAddress, paymentMethod };
                await processCheckout(`${API_BASE}/orders/checkout-cart/${customerId}`, payload);
            }
        });
    }
});

async function processCheckout(url, payload) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            alert("Đặt hàng thành công!");
            window.location.href = "/Storefront/OrderHistory";
        } else {
            const err = await response.text();
            alert("Đặt hàng thất bại: " + err);
        }
    } catch (e) {
        console.error(e);
        alert("Lỗi kết nối.");
    }
}
