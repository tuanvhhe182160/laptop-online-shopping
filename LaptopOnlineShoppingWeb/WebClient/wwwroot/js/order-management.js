const API_BASE = document.body.getAttribute('data-api-base') || 'https://localhost:7136';
const TOKEN = document.body.getAttribute('data-token');
async function updateOrderStatus(orderId, currentStatus) {
    const newStatus = prompt(`Cập nhật trạng thái cho Đơn hàng #${orderId}.\nTrạng thái hiện tại: ${currentStatus}\nNhập trạng thái mới (Pending, Processing, Shipped, Cancelled, Completed):`, currentStatus);
    
    if (!newStatus || newStatus === currentStatus) return;

    // Tùy thuộc logic, giả sử nếu Completed thì cập nhật PaymentStatus = true
    let paymentStatus = false;
    if (newStatus === "Completed" || newStatus === "Shipped") {
        const confirmPayment = confirm("Đơn hàng này đã thanh toán chưa? (OK = Đã thanh toán, Cancel = Chưa)");
        paymentStatus = confirmPayment;
    }

    const payload = {
        orderStatus: newStatus,
        paymentStatus: paymentStatus
    };

    const headers = { 'Content-Type': 'application/json' };
    if (TOKEN) headers['Authorization'] = `Bearer ${TOKEN}`;

    try {
        const response = await fetch(`${API_BASE}/api/orders/${orderId}/status`, {
            method: 'PATCH',
            headers: headers,
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            alert("Cập nhật thành công");
            location.reload();
        } else {
            alert("Cập nhật thất bại. Vui lòng kiểm tra lại trạng thái hợp lệ.");
        }
    } catch (e) {
        console.error(e);
        alert("Lỗi kết nối API.");
    }
}
